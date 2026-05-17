using Microsoft.AspNetCore.Mvc;
using CityLeisure.Api.Models;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/event-categories")]
public class EventCategoriesController : ControllerBase
{
    private readonly IEventCategoryService _eventCategoryService;

    public EventCategoriesController(IEventCategoryService eventCategoryService)
    {
        _eventCategoryService = eventCategoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventCategory>>> GetCategories()
    {
        return await _eventCategoryService.GetCategoriesAsync();
    }
}

