using System.Globalization;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Mapping;

public static class VenueMapUrlBuilder
{
    public static string? BuildYandexUrl(Venue? v)
    {
        if (v == null) return null;
        if (!string.IsNullOrWhiteSpace(v.MapUrl)) return v.MapUrl.Trim();
        if (v.Latitude.HasValue && v.Longitude.HasValue)
        {
            var la = v.Latitude.Value;
            var lo = v.Longitude.Value;
            if (double.IsFinite(la) && double.IsFinite(lo))
                return $"https://yandex.ru/maps/?pt={lo.ToString(CultureInfo.InvariantCulture)},{la.ToString(CultureInfo.InvariantCulture)}&z=16&l=map";
        }
        var parts = new[] { v.Address, v.City }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (parts.Length == 0) return null;
        return "https://yandex.ru/maps/?text=" + Uri.EscapeDataString(string.Join(", ", parts));
    }
}
