namespace CityLeisure.Api.DTOs;

public class AddToCartDto
{
    public int EventId { get; set; }
    public int Quantity { get; set; } = 1;

    public List<string>? Seats { get; set; }
}


