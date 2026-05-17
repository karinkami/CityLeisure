using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Security;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.GetRequiredUserId();

        var user = await _userProfileService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserDto updateDto)
    {
        var userId = User.GetRequiredUserId();
        var result = await _userProfileService.UpdateCurrentUserAsync(userId, updateDto);
        if (result.IsNotFound)
        {
            return NotFound();
        }

        if (result.IsDuplicateEmail)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return result.Data!;
    }
}


