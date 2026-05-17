using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public sealed class FavoriteEventsService : IFavoriteEventsService
{
    private readonly AppDbContext _context;

    public FavoriteEventsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<int>> GetFavoriteEventIdsAsync(int userId)
    {
        return await _context.FavoriteEvents
            .Where(f => f.UserId == userId)
            .Select(f => f.EventId)
            .ToListAsync();
    }

    public async Task<List<Event>> GetFavoriteEventsDetailsAsync(int userId)
    {
        var ids = await _context.FavoriteEvents
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.EventId)
            .ToListAsync();

        if (ids.Count == 0)
        {
            return new List<Event>();
        }

        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => ids.Contains(e.Id))
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<FavoriteMutationResult> AddFavoriteAsync(int userId, int eventId)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return new FavoriteMutationResult
            {
                IsNotFound = true,
                ErrorMessage = "Мероприятие не найдено"
            };
        }

        var exists = await _context.FavoriteEvents
            .AnyAsync(f => f.UserId == userId && f.EventId == eventId);
        if (exists)
        {
            return new FavoriteMutationResult();
        }

        _context.FavoriteEvents.Add(new FavoriteEvent
        {
            UserId = userId,
            EventId = eventId
        });
        await _context.SaveChangesAsync();

        return new FavoriteMutationResult();
    }

    public async Task RemoveFavoriteAsync(int userId, int eventId)
    {
        var favorite = await _context.FavoriteEvents
            .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

        if (favorite == null)
        {
            return;
        }

        _context.FavoriteEvents.Remove(favorite);
        await _context.SaveChangesAsync();
    }
}
