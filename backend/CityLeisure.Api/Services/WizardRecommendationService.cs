using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public interface IWizardRecommendationService
{
    Task<WizardRecommendationResponseDto> RankAsync(WizardRecommendationRequestDto request, int? userId, CancellationToken cancellationToken = default);
}

public sealed class WizardRecommendationService : IWizardRecommendationService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "и", "в", "во", "на", "по", "к", "с", "со", "за", "от", "до", "о", "об", "а", "но", "или",
        "для", "что", "как", "из", "под", "над", "это", "этот", "эта", "эти", "очень", "хочу"
    };

    private readonly AppDbContext _db;

    public WizardRecommendationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WizardRecommendationResponseDto> RankAsync(
        WizardRecommendationRequestDto request,
        int? userId,
        CancellationToken cancellationToken = default)
    {
        var today = EventCatalogQuery.TodayInMoscow();
        var rawEvents = await _db.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.EventDate >= today)
            .ToListAsync(cancellationToken);
        var events = rawEvents
            .Where(e => !string.IsNullOrWhiteSpace(e.Status) &&
                        e.Status.Trim().Equals("active", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var fromDate = EventCatalogQuery.ParseCatalogDateToUtc(request.DateFrom);
        var toDate = EventCatalogQuery.ParseCatalogDateToUtc(request.DateTo);
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            (fromDate, toDate) = (toDate, fromDate);

        var catIds = request.CategoryIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
        var filtered = events.Where(e =>
        {
            if (catIds.Count > 0 && !catIds.Contains(e.CategoryId)) return false;
            if (request.MinPrice is > 0 && e.Price < request.MinPrice.Value) return false;
            if (request.MaxPrice is > 0 && e.Price > request.MaxPrice.Value) return false;
            if (fromDate.HasValue && e.EventDate < fromDate.Value) return false;
            if (toDate.HasValue && e.EventDate >= EventCatalogQuery.CatalogDayEndExclusiveUtc(toDate.Value)) return false;
            return true;
        }).ToList();

        var poolCount = filtered.Count;
        if (poolCount == 0)
            return new WizardRecommendationResponseDto { FilteredPoolCount = 0 };

        var popularity = await _db.OrderItems
            .AsNoTracking()
            .GroupBy(oi => oi.EventId)
            .Select(g => new { EventId = g.Key, Sold = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.EventId, x => x.Sold, cancellationToken);
        var maxSold = popularity.Count == 0 ? 1 : Math.Max(1, popularity.Values.Max());

        var categoryLabels = await _db.EventCategories.AsNoTracking()
            .Where(c => catIds.Contains(c.Id))
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);

        var userIntent = BuildUserIntentText(request, categoryLabels);
        var eventTexts = filtered.Select(BuildEventText).ToList();
        var vectors = BuildTfIdfVectors(userIntent, eventTexts);

        var queryTokens = Tokenize(userIntent);
        var hasStrongIntent = queryTokens.Count >= 3;
        var strict = (request.Strictness ?? "balanced").Trim().ToLowerInvariant() switch
        {
            "soft" => "soft",
            "strict" => "strict",
            _ => "balanced"
        };

        var scored = new List<ScoreRow>();
        foreach (var (ev, idx) in filtered.Select((e, i) => (e, i)))
        {
            var textScore = CosineSimilarity(vectors.User, vectors.Events[idx]);
            var titleOverlap = TitleQueryOverlap(ev, queryTokens);
            textScore = Math.Min(1.0, textScore + titleOverlap * 0.18);

            var budgetScore = BudgetScore(ev, request);
            var timeScore = TimePreferenceScore(ev, request);
            var energyScore = EnergyScore(ev, request);
            var formatScore = FormatScore(ev, request);
            var ageScore = AgeScore(ev, request);

            const double wText = 0.52, wBudget = 0.12, wTime = 0.1, wEnergy = 0.1, wFormat = 0.08, wAge = 0.08;
            var content =
                textScore * wText +
                budgetScore * wBudget +
                timeScore * wTime +
                energyScore * wEnergy +
                formatScore * wFormat +
                ageScore * wAge;

            var sold = popularity.GetValueOrDefault(ev.Id, 0);
            var popularityNorm = sold <= 0 ? 0.0 : Math.Sqrt(Math.Min(1.0, sold / (double)maxSold));

            var blended = 0.70 * content + 0.30 * popularityNorm;

            var daysUntil = (ev.EventDate - today).TotalDays;
            var recency = 0.94 + 0.06 * Math.Exp(-Math.Max(0, daysUntil) / 28.0);
            var finalScore = blended * recency;

            finalScore = Math.Clamp(finalScore, 0, 1.5);
            scored.Add(new ScoreRow(ev, finalScore, textScore, budgetScore, timeScore, energyScore, formatScore, ageScore));
        }

        scored.Sort((a, b) => b.Final.CompareTo(a.Final));

        var threshold = strict switch
        {
            "soft" => hasStrongIntent ? 0.13 : 0.07,
            "strict" => hasStrongIntent ? 0.26 : 0.16,
            _ => hasStrongIntent ? 0.18 : 0.10
        };

        var quality = scored.Where(s => s.Final >= threshold).ToList();
        var backup = strict == "strict" ? 8 : 12;
        var pool = quality.Count >= 3 ? quality : scored.Take(Math.Min(backup, scored.Count)).ToList();

        var limit = Math.Clamp(request.ResultLimit <= 0 ? 9 : request.ResultLimit, 3, 15);
        var diversified = request.Diversify ? ApplyDiversity(pool, limit) : pool.Take(limit).ToList();

        diversified.Sort((a, b) => b.Final.CompareTo(a.Final));

        var result = diversified.Select(s => new WizardRankedEventDto
        {
            Event = s.Event,
            Reason = BuildReason(s, catIds)
        }).ToList();

        if (result.Count == 0 && scored.Count > 0)
        {
            result = scored
                .Take(Math.Min(limit, scored.Count))
                .Select(s => new WizardRankedEventDto
                {
                    Event = s.Event,
                    Reason = BuildReason(s, catIds)
                })
                .ToList();
        }

        return new WizardRecommendationResponseDto
        {
            Events = result,
            FilteredPoolCount = poolCount
        };
    }

    private static string BuildReason(ScoreRow s, List<int> selectedCats)
    {
        var reasons = new List<string>();
        if (s.Text >= 0.2) reasons.Add("сильное совпадение с запросом");
        if (s.Budget >= 0.6) reasons.Add("удобно по бюджету");
        if (s.Time >= 0.9) reasons.Add("подходит по времени");
        if (s.Energy >= 0.9) reasons.Add("под ваш ритм отдыха");
        if (s.Format >= 0.9) reasons.Add("подходит по формату");
        if (s.Age >= 0.9) reasons.Add("под возрастной формат");
        if (selectedCats.Count > 0 && selectedCats.Contains(s.Event.CategoryId))
            reasons.Add("выбранная категория");
        if (reasons.Count == 0) reasons.Add("подходит под ваши ответы");
        return string.Join(" • ", reasons.Take(2));
    }

    private static List<ScoreRow> ApplyDiversity(List<ScoreRow> ordered, int limit)
    {
        var buckets = ordered
            .GroupBy(s => s.Event.CategoryId)
            .OrderByDescending(g => g.Count())
            .Select(g => new Queue<ScoreRow>(g))
            .ToList();

        var result = new List<ScoreRow>(limit);
        var hasNext = true;
        while (hasNext && result.Count < limit)
        {
            hasNext = false;
            foreach (var q in buckets)
            {
                if (result.Count >= limit) break;
                if (q.Count <= 0) continue;
                result.Add(q.Dequeue());
                hasNext = true;
            }
        }

        if (result.Count < limit)
        {
            var seen = result.Select(r => r.Event.Id).ToHashSet();
            foreach (var row in ordered)
            {
                if (result.Count >= limit) break;
                if (seen.Add(row.Event.Id))
                    result.Add(row);
            }
        }

        return result;
    }

    private static double TitleQueryOverlap(Event ev, List<string> queryTokens)
    {
        if (queryTokens.Count == 0) return 0;
        var titleTokens = new HashSet<string>(Tokenize(ev.Title), StringComparer.OrdinalIgnoreCase);
        if (titleTokens.Count == 0) return 0;
        var hit = queryTokens.Count(t => titleTokens.Contains(t));
        return hit / (double)Math.Max(3, queryTokens.Count);
    }

    private sealed record ScoreRow(
        Event Event,
        double Final,
        double Text,
        double Budget,
        double Time,
        double Energy,
        double Format,
        double Age);

    private static string BuildUserIntentText(WizardRecommendationRequestDto r, List<string> categoryNames)
    {
        static string JoinMap(IEnumerable<string> keys, IReadOnlyDictionary<string, string> map) =>
            string.Join(" ", keys.Select(k => map.GetValueOrDefault(k, "")).Where(s => s.Length > 0));

        var companyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["solo"] = "индивидуально спокойно личный отдых",
            ["friends"] = "друзья общение ярко динамично",
            ["family"] = "семья дети безопасно познавательно",
            ["date"] = "романтика уют вечер вдвоем"
        };
        var formatMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["indoor"] = "в помещении зал театр музей",
            ["outdoor"] = "на улице парк open air открытая площадка"
        };
        var timeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["morning"] = "утро раннее время",
            ["day"] = "день дневное время",
            ["evening"] = "вечер позднее время"
        };
        var energyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["calm"] = "спокойно расслабленно культурно",
            ["balanced"] = "умеренно интересно",
            ["active"] = "активно энергично драйв"
        };
        var ageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["kids"] = "0+ 6+ дети семейное",
            ["teen"] = "12+ подростки",
            ["adult"] = "16+ 18+ взрослые"
        };

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(r.Query)) parts.Add(r.Query.Trim());
        parts.Add(JoinMap(r.Company ?? new List<string>(), companyMap));
        parts.Add(JoinMap(r.Format ?? new List<string>(), formatMap));
        parts.Add(JoinMap(r.TimeOfDay ?? new List<string>(), timeMap));
        parts.Add(JoinMap(r.Energy ?? new List<string>(), energyMap));
        parts.Add(JoinMap(r.AgePreference ?? new List<string>(), ageMap));
        if (categoryNames.Count > 0)
            parts.Add(string.Join(" ", categoryNames));
        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static string BuildEventText(Event e)
    {
        var sb = new StringBuilder();
        sb.Append(e.Title).Append(' ');
        sb.Append(e.Description).Append(' ');
        if (e.Category?.Name != null) sb.Append(e.Category.Name).Append(' ');
        if (e.Venue?.Name != null) sb.Append(e.Venue.Name).Append(' ');
        sb.Append(e.AgeRating);
        return sb.ToString();
    }

    private static List<string> Tokenize(string text)
    {
        var norm = Regex.Replace((text ?? "").ToLowerInvariant(), @"[^a-zа-яё0-9+\s-]", " ");
        return Regex.Split(norm.Trim(), @"\s+", RegexOptions.None)
            .Where(t => t.Length > 1 && !StopWords.Contains(t))
            .ToList();
    }

    private static (double[] User, List<double[]> Events) BuildTfIdfVectors(string userDoc, IReadOnlyList<string> eventDocs)
    {
        var tokenized = new List<List<string>> { Tokenize(userDoc) };
        tokenized.AddRange(eventDocs.Select(Tokenize));

        var vocab = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var tokens in tokenized)
        {
            foreach (var t in tokens.Distinct(StringComparer.Ordinal))
            {
                if (!vocab.ContainsKey(t))
                    vocab[t] = vocab.Count;
            }
        }

        var docCount = tokenized.Count;
        var df = new int[vocab.Count];
        foreach (var tokens in tokenized)
        {
            foreach (var t in new HashSet<string>(tokens, StringComparer.Ordinal))
                df[vocab[t]]++;
        }

        var idf = df.Select(v => Math.Log((1 + docCount) / (1.0 + v)) + 1.0).ToArray();

        double[] VectorFor(List<string> tokens)
        {
            var vec = new double[vocab.Count];
            if (tokens.Count == 0) return vec;
            var tf = tokens.GroupBy(x => x, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
            foreach (var (token, cnt) in tf)
            {
                if (!vocab.TryGetValue(token, out var idx)) continue;
                vec[idx] = cnt / (double)tokens.Count * idf[idx];
            }

            return vec;
        }

        var userVec = VectorFor(tokenized[0]);
        var eventVecs = tokenized.Skip(1).Select(VectorFor).ToList();
        return (userVec, eventVecs);
    }

    private static double CosineSimilarity(IReadOnlyList<double> a, IReadOnlyList<double> b)
    {
        var n = Math.Min(a.Count, b.Count);
        double dot = 0, ma = 0, mb = 0;
        for (var i = 0; i < n; i++)
        {
            dot += a[i] * b[i];
            ma += a[i] * a[i];
            mb += b[i] * b[i];
        }

        if (ma <= 0 || mb <= 0) return 0;
        return dot / (Math.Sqrt(ma) * Math.Sqrt(mb));
    }

    private static double BudgetScore(Event e, WizardRecommendationRequestDto r)
    {
        var min = r.MinPrice ?? 0;
        var max = r.MaxPrice ?? 0;
        var price = (double)e.Price;
        if (min > 0 && max > 0 && max >= min)
        {
            var center = ((double)min + (double)max) / 2.0;
            var radius = Math.Max(1.0, ((double)max - (double)min) / 2.0);
            return Math.Max(0, 1.0 - Math.Abs(price - center) / radius);
        }

        if (max > 0)
            return Math.Max(0, 1.0 - price / Math.Max(1.0, (double)max));
        return 0.5;
    }

    private static double TimePreferenceScore(Event e, WizardRecommendationRequestDto r)
    {
        var tod = r.TimeOfDay ?? new List<string>();
        if (tod.Count == 0) return 0.5;
        var hour = (int)e.EventTime.TotalHours;
        if (hour < 0 || hour > 23) hour = 12;
        bool Match(string x) => x switch
        {
            "morning" => hour >= 7 && hour < 12,
            "day" => hour >= 12 && hour < 18,
            _ => hour >= 18 && hour <= 23
        };
        return tod.Any(Match) ? 1.0 : 0.2;
    }

    private static double EnergyScore(Event e, WizardRecommendationRequestDto request)
    {
        var opts = request.Energy ?? new List<string>();
        if (opts.Count == 0) return 0.5;
        var blob = BuildEventText(e).ToLowerInvariant();
        double One(string energy) => energy switch
        {
            "calm" => CalmRx.IsMatch(blob) ? 1.0 : 0.25,
            "active" => ActiveRx.IsMatch(blob) ? 1.0 : 0.25,
            _ => MidRx.IsMatch(blob) ? 0.9 : 0.45
        };
        return opts.Max(One);
    }

    private static readonly Regex CalmRx = new(@"(музей|выставк|театр|лекци|джаз|экскурс)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ActiveRx = new(@"(фестив|концерт|шоу|спорт|клуб|танц)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MidRx = new(@"(концерт|театр|выставк|фестиваль|кино|лекци)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static double FormatScore(Event e, WizardRecommendationRequestDto request)
    {
        var opts = request.Format ?? new List<string>();
        if (opts.Count == 0) return 0.5;
        var blob = BuildEventText(e).ToLowerInvariant();
        var indoor = IndoorRx.IsMatch(blob);
        var outdoor = OutdoorRx.IsMatch(blob);
        double One(string f) => f.Equals("indoor", StringComparison.OrdinalIgnoreCase)
            ? (indoor ? 1.0 : 0.35)
            : (outdoor ? 1.0 : 0.35);
        return opts.Max(One);
    }

    private static readonly Regex IndoorRx = new(@"(театр|музей|зал|дворец|галерея|кино)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OutdoorRx = new(@"(парк|набереж|open air|улич|площадь)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static double AgeScore(Event e, WizardRecommendationRequestDto request)
    {
        var opts = request.AgePreference ?? new List<string>();
        if (opts.Count == 0) return 0.5;
        var rating = (e.AgeRating ?? "").ToLowerInvariant();
        double One(string ap) => ap switch
        {
            "kids" => KidRx.IsMatch(rating) ? 1.0 : 0.1,
            "teen" => TeenRx.IsMatch(rating) ? 1.0 : 0.25,
            _ => AdultRx.IsMatch(rating) ? 1.0 : 0.35
        };
        return opts.Max(One);
    }

    private static readonly Regex KidRx = new(@"(0\+|6\+)", RegexOptions.Compiled);
    private static readonly Regex TeenRx = new(@"(12\+)", RegexOptions.Compiled);
    private static readonly Regex AdultRx = new(@"(16\+|18\+)", RegexOptions.Compiled);
}
