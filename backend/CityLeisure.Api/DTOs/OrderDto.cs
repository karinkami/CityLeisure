namespace CityLeisure.Api.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public List<string>? SeatLabels { get; set; }
    public EventDto? Event { get; set; }
}


