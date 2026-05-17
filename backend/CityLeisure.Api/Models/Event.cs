namespace CityLeisure.Api.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int VenueId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableTickets { get; set; }
    public string AgeRating { get; set; } = "0+";
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string SeatingType { get; set; } = "general";

    public string? SeatLayoutJson { get; set; }

    public EventCategory? Category { get; set; }
    public Venue? Venue { get; set; }
}

