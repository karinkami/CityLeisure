using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public interface IEventRecommendationService
{
    Task<IReadOnlyList<Event>> GetRecommendationsAsync(int? userId, int limit = 6, CancellationToken cancellationToken = default);
}

public class EventRecommendationService : IEventRecommendationService
{
    private const decimal CategoryWeight = 3.6m;
    private const decimal VenueWeight = 1.9m;
    private const decimal PriceWeight = 1.1m;
    private const decimal PopularityWeight = 1.6m;
    private readonly AppDbContext _context;

    public EventRecommendationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Event>> GetRecommendationsAsync(int? userId, int limit = 6, CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 300);
        var today = DateTime.UtcNow.Date;

        var activeEvents = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.Status == "active" && e.EventDate >= today)
            .ToListAsync(cancellationToken);

        if (activeEvents.Count == 0)
        {
            return Array.Empty<Event>();
        }

        var popularityMap = await _context.OrderItems
            .GroupBy(oi => oi.EventId)
            .Select(g => new { EventId = g.Key, SoldTickets = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.EventId, x => x.SoldTickets, cancellationToken);

        var maxSoldTickets = popularityMap.Count == 0 ? 1 : Math.Max(1, popularityMap.Values.Max());

        List<Event> ordered;
        if (!userId.HasValue || userId.Value <= 0)
        {
            ordered = activeEvents
                .OrderByDescending(e => GetGlobalScore(e, popularityMap, maxSoldTickets))
                .ThenBy(e => e.Id)
                .ToList();
        }
        else
        {
            var profile = await BuildUserProfileAsync(userId.Value, cancellationToken);

            if (profile.InteractedEventIds.Count == 0)
            {
                ordered = activeEvents
                    .OrderByDescending(e => GetGlobalScore(e, popularityMap, maxSoldTickets))
                    .ThenBy(e => e.Id)
                    .ToList();
            }
            else
            {
                ordered = activeEvents
                    .Where(e => !profile.InteractedEventIds.Contains(e.Id))
                    .OrderByDescending(e => GetPersonalScore(e, profile, popularityMap, maxSoldTickets))
                    .ThenBy(e => e.Id)
                    .ToList();
            }
        }

        if (ordered.Count <= safeLimit)
            return ordered;

        var poolSize = Math.Min(ordered.Count, Math.Max(safeLimit * 4, safeLimit));
        var pool = ordered.Take(poolSize).ToList();
        return DiversifyByCategoryRoundRobin(pool, safeLimit);
    }

    private static List<Event> DiversifyByCategoryRoundRobin(IReadOnlyList<Event> pool, int limit)
    {
        var queues = pool
            .GroupBy(e => e.CategoryId)
            .OrderByDescending(g => g.Count())
            .Select(g => new Queue<Event>(g))
            .ToList();

        var result = new List<Event>(limit);
        while (result.Count < limit && queues.Any(q => q.Count > 0))
        {
            foreach (var q in queues)
            {
                if (result.Count >= limit)
                    break;
                if (q.Count > 0)
                    result.Add(q.Dequeue());
            }
        }

        if (result.Count < limit)
        {
            var seen = result.Select(e => e.Id).ToHashSet();
            foreach (var e in pool)
            {
                if (result.Count >= limit)
                    break;
                if (seen.Add(e.Id))
                    result.Add(e);
            }
        }

        return result;
    }

    private static decimal GetGlobalScore(Event eventItem, IReadOnlyDictionary<int, int> popularityMap, int maxSoldTickets)
    {
        var popularityScore = 0m;
        if (popularityMap.TryGetValue(eventItem.Id, out var soldTickets) && soldTickets > 0 && maxSoldTickets > 0)
        {
            var ratio = (double)soldTickets / maxSoldTickets;
            popularityScore = (decimal)Math.Sqrt(Math.Min(1.0, ratio));
        }

        return popularityScore * PopularityWeight;
    }

    private decimal GetPersonalScore(
        Event eventItem,
        UserProfile profile,
        IReadOnlyDictionary<int, int> popularityMap,
        int maxSoldTickets)
    {
        var categoryAffinity = profile.CategoryAffinity.TryGetValue(eventItem.CategoryId, out var categoryScore)
            ? categoryScore
            : 0m;

        var venueAffinity = profile.VenueAffinity.TryGetValue(eventItem.VenueId, out var venueScore)
            ? venueScore
            : 0m;

        var popularityScore = 0m;
        if (popularityMap.TryGetValue(eventItem.Id, out var soldTickets) && soldTickets > 0 && maxSoldTickets > 0)
        {
            var ratio = (double)soldTickets / maxSoldTickets;
            popularityScore = (decimal)Math.Sqrt(Math.Min(1.0, ratio));
        }

        var priceScore = profile.AveragePrice <= 0m
            ? 0m
            : 1m / (1m + Math.Abs(eventItem.Price - profile.AveragePrice) / Math.Max(profile.AveragePrice, 1m));

        var personalizationStrength = Math.Clamp(profile.TotalInteractionWeight / 12m, 0.25m, 1m);
        var personalBoost = 1m + personalizationStrength * 0.45m;
        var popularityFactor = 1m - personalizationStrength * 0.55m;

        var score = categoryAffinity * CategoryWeight * personalBoost
                    + venueAffinity * VenueWeight * personalBoost
                    + priceScore * PriceWeight
                    + popularityScore * PopularityWeight * popularityFactor;

        return score;
    }

    private async Task<UserProfile> BuildUserProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var favoriteEvents = await _context.FavoriteEvents
            .Where(f => f.UserId == userId)
            .Join(_context.Events, favorite => favorite.EventId, ev => ev.Id, (_, ev) => ev)
            .ToListAsync(cancellationToken);

        var cartEvents = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Join(_context.Events, cart => cart.EventId, ev => ev.Id, (_, ev) => ev)
            .ToListAsync(cancellationToken);

        var purchasedEvents = await _context.OrderItems
            .Where(oi => oi.Order != null && oi.Order.UserId == userId)
            .Join(_context.Events, oi => oi.EventId, ev => ev.Id, (_, ev) => ev)
            .ToListAsync(cancellationToken);

        var weightedEvents = new List<(Event Event, decimal Weight)>();
        weightedEvents.AddRange(favoriteEvents.Select(e => (e, 4.2m)));
        weightedEvents.AddRange(cartEvents.Select(e => (e, 2.6m)));
        weightedEvents.AddRange(purchasedEvents.Select(e => (e, 3.8m)));

        var interactedEventIds = weightedEvents
            .Select(x => x.Event.Id)
            .ToHashSet();

        if (weightedEvents.Count == 0)
        {
            return UserProfile.Empty;
        }

        var totalWeight = weightedEvents.Sum(x => x.Weight);
        var averagePrice = weightedEvents.Sum(x => x.Event.Price * x.Weight) / totalWeight;

        var categoryAffinity = weightedEvents
            .GroupBy(x => x.Event.CategoryId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Weight) / totalWeight);

        var venueAffinity = weightedEvents
            .GroupBy(x => x.Event.VenueId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Weight) / totalWeight);

        return new UserProfile(interactedEventIds, categoryAffinity, venueAffinity, averagePrice, totalWeight);
    }

    private sealed record UserProfile(
        HashSet<int> InteractedEventIds,
        Dictionary<int, decimal> CategoryAffinity,
        Dictionary<int, decimal> VenueAffinity,
        decimal AveragePrice,
        decimal TotalInteractionWeight)
    {
        public static readonly UserProfile Empty = new(
            new HashSet<int>(),
            new Dictionary<int, decimal>(),
            new Dictionary<int, decimal>(),
            0m,
            0m);
    }
}

