using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;
using CityLeisure.Api.Security;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/favorite-events")]
[Authorize]
public class FavoriteEventsController : ControllerBase
{
    private readonly IFavoriteEventsService _favoriteEventsService;

    public FavoriteEventsController(IFavoriteEventsService favoriteEventsService)
    {
        _favoriteEventsService = favoriteEventsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<int>>> GetFavoriteEventIds()
    {
        var userId = User.GetRequiredUserId();
        var ids = await _favoriteEventsService.GetFavoriteEventIdsAsync(userId);

        return Ok(ids);
    }

    [HttpGet("details")]
    public async Task<ActionResult<IEnumerable<Event>>> GetFavoriteEventsDetails()
    {
        var userId = User.GetRequiredUserId();
        var events = await _favoriteEventsService.GetFavoriteEventsDetailsAsync(userId);

        return Ok(events);
    }

    [HttpPost]
    public async Task<ActionResult> AddFavorite([FromBody] FavoriteEventDto dto)
    {
        var userId = User.GetRequiredUserId();
        var result = await _favoriteEventsService.AddFavoriteAsync(userId, dto.EventId);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok();
    }

    [HttpDelete("{eventId:int}")]
    public async Task<ActionResult> RemoveFavorite(int eventId)
    {
        var userId = User.GetRequiredUserId();
        await _favoriteEventsService.RemoveFavoriteAsync(userId, eventId);
        return NoContent();
    }
}

