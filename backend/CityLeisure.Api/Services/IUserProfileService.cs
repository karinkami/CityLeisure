using CityLeisure.Api.DTOs;

namespace CityLeisure.Api.Services;

public interface IUserProfileService
{
    Task<UserDto?> GetCurrentUserAsync(int userId);
    Task<UserProfileUpdateResult> UpdateCurrentUserAsync(int userId, UpdateUserDto updateDto);
}

public sealed class UserProfileUpdateResult
{
    public bool IsNotFound { get; init; }
    public bool IsDuplicateEmail { get; init; }
    public string? ErrorMessage { get; init; }
    public UserDto? Data { get; init; }
}
