using System.Text.Json.Serialization;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.DTOs;

public class WizardRecommendationRequestDto
{
    public string? Query { get; set; }
    public List<string> Company { get; set; } = new();
    public List<string> Format { get; set; } = new();
    public List<string> TimeOfDay { get; set; } = new();
    public List<string> Energy { get; set; } = new();
    public List<string> AgePreference { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public string Strictness { get; set; } = "balanced";
    public int ResultLimit { get; set; } = 9;
    public bool Diversify { get; set; } = true;
}

public class WizardRecommendationResponseDto
{
    public List<WizardRankedEventDto> Events { get; set; } = new();
    public int FilteredPoolCount { get; set; }
}

public class WizardRankedEventDto
{
    [JsonPropertyName("eventItem")]
    public Event Event { get; set; } = null!;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = "";
}
