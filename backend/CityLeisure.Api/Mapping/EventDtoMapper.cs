using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Mapping;

public static class EventDtoMapper
{
    public static EventDto FromEvent(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        CategoryName = e.Category?.Name ?? string.Empty,
        VenueName = e.Venue?.Name ?? string.Empty,
        VenueAddress = e.Venue?.Address ?? string.Empty,
        VenueCity = e.Venue?.City ?? string.Empty,
        VenueLatitude = e.Venue?.Latitude,
        VenueLongitude = e.Venue?.Longitude,
        VenueMapUrl = e.Venue?.MapUrl,
        VenueMapOpenUrl = VenueMapUrlBuilder.BuildYandexUrl(e.Venue),
        EventDate = e.EventDate,
        EventTime = e.EventTime,
        Price = e.Price,
        AvailableTickets = e.AvailableTickets,
        AgeRating = e.AgeRating,
        ImageUrl = e.ImageUrl,
        SeatingType = e.SeatingType
    };
}
