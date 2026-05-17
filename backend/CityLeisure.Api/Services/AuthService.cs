using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            return null;
        }
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
        var user = new User
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var role = await SyncUserRoleAsync(user);
        var token = GenerateToken(user, role);

        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = role,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<(AuthResponseDto? Response, string? ErrorCode)> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        
        if (user == null)
        {
            return (null, "account_not_found");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return (null, "invalid_password");
        }

        var role = await SyncUserRoleAsync(user);
        var token = GenerateToken(user, role);

        return (new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = role,
                CreatedAt = user.CreatedAt
            }
        }, null);
    }

    private string GenerateToken(User user, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "your-secret-key-that-is-at-least-32-characters-long"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "CityLeisure",
            audience: _configuration["Jwt:Audience"] ?? "CityLeisure",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string NormalizeRole(string? role)
    {
        return string.Equals(role?.Trim(), "Admin", StringComparison.OrdinalIgnoreCase)
            ? "Admin"
            : "User";
    }

    private async Task<string> SyncUserRoleAsync(User user)
    {
        var targetRole = IsAdminEmail(user.Email) ? "Admin" : "User";
        if (!string.Equals(user.Role, targetRole, StringComparison.OrdinalIgnoreCase))
        {
            user.Role = targetRole;
            await _context.SaveChangesAsync();
        }
        return targetRole;
    }

    private bool IsAdminEmail(string email)
    {
        var adminEmails = _configuration
            .GetSection("Admin:Emails")
            .Get<string[]>() ?? Array.Empty<string>();

        return adminEmails.Any(e => string.Equals(e?.Trim(), email, StringComparison.OrdinalIgnoreCase));
    }
}


