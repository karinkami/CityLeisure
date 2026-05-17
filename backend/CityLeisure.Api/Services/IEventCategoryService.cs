using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public interface IEventCategoryService
{
    Task<List<EventCategory>> GetCategoriesAsync();
}
