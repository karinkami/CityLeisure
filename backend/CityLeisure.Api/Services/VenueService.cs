using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public sealed class VenueService : IVenueService
{
    private readonly AppDbContext _context;

    public VenueService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venue>> GetVenuesAsync()
    {
        return await _context.Venues.AsNoTracking().OrderBy(v => v.Name).ToListAsync();
    }

    public async Task<VenueUpdateResult> UpdateVenueAsync(int id, UpdateVenueAdminDto dto)
    {
        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Id == id);
        if (venue == null)
        {
            return new VenueUpdateResult
            {
                IsNotFound = true,
                ErrorMessage = "Площадка не найдена"
            };
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
            venue.Name = dto.Name.Trim();
        if (dto.Address != null)
            venue.Address = dto.Address.Trim();
        if (dto.City != null)
            venue.City = dto.City.Trim();
        if (dto.Description != null)
            venue.Description = dto.Description.Trim();
        if (dto.MapUrl != null)
            venue.MapUrl = string.IsNullOrWhiteSpace(dto.MapUrl) ? null : dto.MapUrl.Trim();

        await _context.SaveChangesAsync();

        return new VenueUpdateResult
        {
            Data = venue
        };
    }
}
