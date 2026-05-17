using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public interface IFavoriteEventsService
{
    Task<List<int>> GetFavoriteEventIdsAsync(int userId);
    Task<List<Event>> GetFavoriteEventsDetailsAsync(int userId);
    Task<FavoriteMutationResult> AddFavoriteAsync(int userId, int eventId);
    Task RemoveFavoriteAsync(int userId, int eventId);
}

public sealed class FavoriteMutationResult
{
    public bool IsNotFound { get; init; }
    public string? ErrorMessage { get; init; }
}
