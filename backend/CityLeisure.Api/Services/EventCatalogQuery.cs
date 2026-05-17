using System.Globalization;
using CityLeisure.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CityLeisure.Api.Services;

public static class EventCatalogQuery
{
    private static readonly TimeZoneInfo MoscowTz = GetMoscowTz();

    private static TimeZoneInfo GetMoscowTz()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    public static DateTime MoscowCalendarStartUtc(DateOnly calendarDayInMoscow)
    {
        var localMidnight = new DateTime(
            calendarDayInMoscow.Year,
            calendarDayInMoscow.Month,
            calendarDayInMoscow.Day,
            0, 0, 0,
            DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localMidnight, MoscowTz);
    }

    public static DateTime TodayInMoscow()
    {
        var moscowNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MoscowTz);
        return MoscowCalendarStartUtc(DateOnly.FromDateTime(moscowNow));
    }

    public static DateTime? ParseCatalogDateToUtc(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var t = value.Trim();
        if (t.Length > 10) t = t[..10];
        return DateOnly.TryParse(t, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)
            ? MoscowCalendarStartUtc(day)
            : null;
    }

    public static DateTime CatalogDayEndExclusiveUtc(DateTime dayStartUtcMoscow)
    {
        var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(dayStartUtcMoscow, MoscowTz));
        return MoscowCalendarStartUtc(day.AddDays(1));
    }

    public static IQueryable<Event> ActiveOnly(IQueryable<Event> source) =>
        source.Where(e => e.Status == "active");

    public static IQueryable<Event> ApplyTitleSearch(IQueryable<Event> query, string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return query;
        var term = q.Trim();
        var pattern = "%" + EscapeILike(term) + "%";
        return query.Where(e => EF.Functions.ILike(e.Title, pattern));
    }

    public static IQueryable<Event> ApplyFilters(
        IQueryable<Event> query,
        int? categoryId,
        int? venueId,
        DateTime? dateFrom,
        DateTime? dateTo,
        decimal? priceMin,
        decimal? priceMax,
        bool? freeOnly,
        bool? paidOnly)
    {
        if (categoryId is > 0)
            query = query.Where(e => e.CategoryId == categoryId.Value);
        if (venueId is > 0)
            query = query.Where(e => e.VenueId == venueId.Value);

        if (dateFrom.HasValue)
        {
            var d = dateFrom.Value;
            query = query.Where(e => e.EventDate >= d);
        }

        if (dateTo.HasValue)
        {
            var d = dateTo.Value;
            var lastDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(d, MoscowTz));
            var endExclusive = MoscowCalendarStartUtc(lastDay.AddDays(1));
            query = query.Where(e => e.EventDate < endExclusive);
        }

        var pMin = priceMin;
        var pMax = priceMax;
        if (pMin.HasValue && pMax.HasValue && pMin.Value > pMax.Value)
            (pMin, pMax) = (pMax, pMin);

        if (pMin.HasValue)
            query = query.Where(e => e.Price >= pMin.Value);
        if (pMax.HasValue)
            query = query.Where(e => e.Price <= pMax.Value);

        if (freeOnly == true)
            query = query.Where(e => e.Price == 0);
        else if (paidOnly == true)
            query = query.Where(e => e.Price > 0);

        return query;
    }

    public static IQueryable<Event> ApplySection(
        IQueryable<Event> query,
        string? section,
        DateTime todayUtcMoscowDayStart)
    {
        if (string.IsNullOrWhiteSpace(section))
            return query;

        var s = section.Trim().ToLowerInvariant();
        var moscowDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(todayUtcMoscowDayStart, MoscowTz));
        var dayStart = MoscowCalendarStartUtc(moscowDate);
        var dayEnd = MoscowCalendarStartUtc(moscowDate.AddDays(1));
        var soonEnd = MoscowCalendarStartUtc(moscowDate.AddDays(31));

        return s switch
        {
            "today" => query.Where(e => e.EventDate >= dayStart && e.EventDate < dayEnd),
            "soon" => query.Where(e => e.EventDate >= dayStart && e.EventDate < soonEnd),
            "weekend" => query.Where(e =>
                e.EventDate >= dayStart &&
                (e.EventDate.DayOfWeek == DayOfWeek.Saturday || e.EventDate.DayOfWeek == DayOfWeek.Sunday)),
            "free" => query.Where(e => e.Price == 0),
            _ => query
        };
    }

    public static IQueryable<Event> ApplyCatalogOrder(IQueryable<Event> query) =>
        query.OrderBy(e => e.Id);

    private static string EscapeILike(string s) =>
        s.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
