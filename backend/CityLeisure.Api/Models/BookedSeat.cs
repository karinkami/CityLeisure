namespace CityLeisure.Api.Models;

public class BookedSeat
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string SeatLabel { get; set; } = string.Empty;
    public int OrderId { get; set; }

    public Event? Event { get; set; }
    public Order? Order { get; set; }
}
