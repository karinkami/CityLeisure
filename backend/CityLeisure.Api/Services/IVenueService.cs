using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public interface IVenueService
{
    Task<List<Venue>> GetVenuesAsync();
    Task<VenueUpdateResult> UpdateVenueAsync(int id, UpdateVenueAdminDto dto);
}

public sealed class VenueUpdateResult
{
    public bool IsNotFound { get; init; }
    public string? ErrorMessage { get; init; }
    public Venue? Data { get; init; }
}
