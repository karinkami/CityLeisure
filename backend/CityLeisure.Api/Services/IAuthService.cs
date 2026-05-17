using CityLeisure.Api.DTOs;

namespace CityLeisure.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<(AuthResponseDto? Response, string? ErrorCode)> LoginAsync(LoginDto loginDto);
}
