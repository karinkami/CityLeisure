using System.Text.Json;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public static class SeatLayoutService
{
    public const string General = "general";
    public const string Assigned = "assigned";

    public static bool UsesAssignedSeats(string seatingType, decimal price) =>
        string.Equals(seatingType, Assigned, StringComparison.OrdinalIgnoreCase) && price > 0m;

    public static bool UsesAssignedSeats(Event e) => UsesAssignedSeats(e.SeatingType, e.Price);

    public static string BuildDefaultCinemaLayoutJson()
    {
        var rows = new List<SeatRowDto>();
        for (var r = 1; r <= 6; r++)
        {
            var seats = Enumerable.Range(1, 10).Select(c => $"{r}-{c}").ToList();
            rows.Add(new SeatRowDto($"Ряд {r}", seats));
        }

        var obj = new { section = "Партер", rows };
        return JsonSerializer.Serialize(obj, JsonOptions());
    }

    public static HashSet<string> ParseValidSeatLabels(string layoutJson)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            using var doc = JsonDocument.Parse(layoutJson);
            if (!doc.RootElement.TryGetProperty("rows", out var rowsEl))
                return set;
            foreach (var row in rowsEl.EnumerateArray())
            {
                if (!row.TryGetProperty("seats", out var seatsEl))
                    continue;
                foreach (var s in seatsEl.EnumerateArray())
                {
                    var label = s.GetString();
                    if (!string.IsNullOrEmpty(label))
                        set.Add(label);
                }
            }
        }
        catch
        {
        }

        return set;
    }

    public static string ResolveLayoutJson(string? seatLayoutJson)
    {
        if (!string.IsNullOrWhiteSpace(seatLayoutJson))
            return seatLayoutJson;
        return BuildDefaultCinemaLayoutJson();
    }

    private static JsonSerializerOptions JsonOptions() =>
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private sealed record SeatRowDto(string RowLabel, List<string> Seats);
}
