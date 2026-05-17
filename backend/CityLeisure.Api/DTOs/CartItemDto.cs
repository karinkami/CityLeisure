namespace CityLeisure.Api.DTOs;

public class CartItemDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int Quantity { get; set; }
    public List<string>? SelectedSeats { get; set; }
    public EventDto? Event { get; set; }
}

public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public string VenueAddress { get; set; } = string.Empty;
    public string VenueCity { get; set; } = string.Empty;
    public double? VenueLatitude { get; set; }
    public double? VenueLongitude { get; set; }
    public string? VenueMapUrl { get; set; }
    public string? VenueMapOpenUrl { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableTickets { get; set; }
    public string AgeRating { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string SeatingType { get; set; } = "general";
}


