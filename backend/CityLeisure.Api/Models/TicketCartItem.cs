namespace CityLeisure.Api.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }
    public int Quantity { get; set; }

    public string? SelectedSeatsJson { get; set; }

    public User? User { get; set; }
    public Event? Event { get; set; }
}

