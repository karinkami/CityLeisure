using Microsoft.AspNetCore.Mvc;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        if (string.IsNullOrWhiteSpace(registerDto.Email) || 
            string.IsNullOrWhiteSpace(registerDto.Password) ||
            string.IsNullOrWhiteSpace(registerDto.UserName))
        {
            return BadRequest(new { message = "Р’СЃРµ РїРѕР»СЏ РѕР±СЏР·Р°С‚РµР»СЊРЅС‹ РґР»СЏ Р·Р°РїРѕР»РЅРµРЅРёСЏ" });
        }

        var result = await _authService.RegisterAsync(registerDto);
        
        if (result == null)
        {
            return BadRequest(new { message = "РџРѕР»СЊР·РѕРІР°С‚РµР»СЊ СЃ С‚Р°РєРёРј email СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚" });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest(new { message = "Email Рё РїР°СЂРѕР»СЊ РѕР±СЏР·Р°С‚РµР»СЊРЅС‹" });
        }

        var (result, errorCode) = await _authService.LoginAsync(loginDto);
        
        if (result == null)
        {
            if (errorCode == "account_not_found")
            {
                return NotFound(new { message = "Аккаунт с таким email не найден" });
            }

            if (errorCode == "invalid_password")
            {
                return Unauthorized(new { message = "Неверный пароль" });
            }

            return Unauthorized(new { message = "Неверный email или пароль" });
        }

        return Ok(result);
    }
}


