namespace CityLeisure.Api.DTOs;

public class SeatMapDto
{
    public string SeatingType { get; set; } = "general";
    public string LayoutJson { get; set; } = "{}";
    public List<string> Booked { get; set; } = new();
}
