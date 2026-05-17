using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;

namespace CityLeisure.Api.Services;

public sealed class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _context;

    public UserProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return null;
        }

        return MapUser(user);
    }

    public async Task<UserProfileUpdateResult> UpdateCurrentUserAsync(int userId, UpdateUserDto updateDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new UserProfileUpdateResult
            {
                IsNotFound = true
            };
        }

        if (!string.IsNullOrWhiteSpace(updateDto.UserName))
        {
            user.UserName = updateDto.UserName;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Email))
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == updateDto.Email && u.Id != userId);

            if (emailExists)
            {
                return new UserProfileUpdateResult
                {
                    IsDuplicateEmail = true,
                    ErrorMessage = "РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃ С‚Р°РєРёРј email СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚"
                };
            }

            user.Email = updateDto.Email;
        }

        await _context.SaveChangesAsync();

        return new UserProfileUpdateResult
        {
            Data = MapUser(user)
        };
    }

    private static UserDto MapUser(Models.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User",
            CreatedAt = user.CreatedAt
        };
    }
}
