using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
    {
        return await _venueService.GetVenuesAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Venue>> UpdateVenue(int id, [FromBody] UpdateVenueAdminDto dto)
    {
        var result = await _venueService.UpdateVenueAsync(id, dto);
        if (result.IsNotFound)
            return NotFound(new { message = "Площадка не найдена" });

        return Ok(result.Data);
    }
}

