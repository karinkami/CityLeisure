using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public sealed class EventCategoryService : IEventCategoryService
{
    private readonly AppDbContext _context;

    public EventCategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventCategory>> GetCategoriesAsync()
    {
        return await _context.EventCategories.AsNoTracking().ToListAsync();
    }
}
