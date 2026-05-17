namespace CityLeisure.Api.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int EventId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public string? SeatLabelsJson { get; set; }

    public Order? Order { get; set; }
    public Event? Event { get; set; }
}


