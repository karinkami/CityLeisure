using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Models;
using CityLeisure.Api.Security;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEventRecommendationService _recommendationService;
    private readonly IWizardRecommendationService _wizardRecommendationService;

    public EventsController(
        AppDbContext context,
        IEventRecommendationService recommendationService,
        IWizardRecommendationService wizardRecommendationService)
    {
        _context = context;
        _recommendationService = recommendationService;
        _wizardRecommendationService = wizardRecommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents(
        [FromQuery] string? q = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? venueId = null,
        [FromQuery] string? dateFrom = null,
        [FromQuery] string? dateTo = null,
        [FromQuery] decimal? priceMin = null,
        [FromQuery] decimal? priceMax = null,
        [FromQuery] bool? freeOnly = null,
        [FromQuery] bool? paidOnly = null,
        [FromQuery] string? section = null)
    {
        try
        {
            var today = EventCatalogQuery.TodayInMoscow();
            var dateFromDt = EventCatalogQuery.ParseCatalogDateToUtc(dateFrom);
            var dateToDt = EventCatalogQuery.ParseCatalogDateToUtc(dateTo);
            if (dateFromDt.HasValue && dateToDt.HasValue && dateFromDt.Value > dateToDt.Value)
                (dateFromDt, dateToDt) = (dateToDt, dateFromDt);

            var baseQuery = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .AsQueryable();

            baseQuery = EventCatalogQuery.ActiveOnly(baseQuery);
            baseQuery = EventCatalogQuery.ApplyTitleSearch(baseQuery, q);
            baseQuery = EventCatalogQuery.ApplyFilters(
                baseQuery, categoryId, venueId, dateFromDt, dateToDt, priceMin, priceMax, freeOnly, paidOnly);

            var sec = section?.Trim().ToLowerInvariant();
            List<Event> events;

            if (sec == "recommended")
            {
                events = await LoadRecommendedSectionAsync(baseQuery, User.GetOptionalUserId());
            }
            else if (sec == "popular")
            {
                events = await LoadPopularSectionAsync(baseQuery);
            }
            else
            {
                var query = EventCatalogQuery.ApplySection(baseQuery, section, today);
                query = EventCatalogQuery.ApplyCatalogOrder(query);
                events = await query.ToListAsync();
            }

            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при загрузке мероприятий", message = ex.Message });
        }
    }

    private async Task<List<Event>> LoadPopularSectionAsync(IQueryable<Event> filteredActive)
    {
        var topEventIds = await _context.OrderItems
            .GroupBy(oi => oi.EventId)
            .Select(g => new { EventId = g.Key, SoldTickets = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.SoldTickets)
            .Take(40)
            .Select(x => x.EventId)
            .ToListAsync();

        if (topEventIds.Count == 0)
        {
            return await filteredActive
                .OrderBy(e => e.Id)
                .Take(24)
                .ToListAsync();
        }

        var list = await filteredActive
            .Where(e => topEventIds.Contains(e.Id))
            .ToListAsync();

        var orderIndex = topEventIds.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        list.Sort((a, b) =>
        {
            var byPop = orderIndex.GetValueOrDefault(a.Id, 999).CompareTo(orderIndex.GetValueOrDefault(b.Id, 999));
            if (byPop != 0) return byPop;
            return a.Id.CompareTo(b.Id);
        });

        return list;
    }

    private async Task<List<Event>> LoadRecommendedSectionAsync(IQueryable<Event> filteredActive, int? userId)
    {
        var filtered = await filteredActive
            .OrderBy(e => e.Id)
            .ToListAsync();

        if (filtered.Count == 0)
            return new List<Event>();

        var byId = filtered.ToDictionary(e => e.Id);
        var idSet = byId.Keys.ToHashSet();

        var recs = await _recommendationService.GetRecommendationsAsync(userId, 200);
        var seen = new HashSet<int>();
        var picked = new List<Event>(filtered.Count);

        foreach (var e in recs)
        {
            if (!idSet.Contains(e.Id) || !seen.Add(e.Id))
                continue;
            if (byId.TryGetValue(e.Id, out var full))
                picked.Add(full);
        }

        foreach (var e in filtered)
        {
            if (!seen.Add(e.Id))
                continue;
            picked.Add(e);
        }

        return picked;
    }

    [HttpGet("upcoming-week")]
    public async Task<ActionResult<IEnumerable<Event>>> GetUpcomingWeekEvents()
    {
        var today = DateTime.UtcNow.Date;
        var lastDay = today.AddDays(7);

        var events = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.Status == "active" && e.EventDate >= today && e.EventDate <= lastDay)
            .OrderBy(e => e.Id)
            .Take(12)
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("top-popular")]
    public async Task<ActionResult<IEnumerable<Event>>> GetTopPopularEvents()
    {
        var topEventIds = await _context.OrderItems
            .GroupBy(oi => oi.EventId)
            .Select(g => new { EventId = g.Key, SoldTickets = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.SoldTickets)
            .Take(10)
            .Select(x => x.EventId)
            .ToListAsync();

        List<Event> result;
        if (topEventIds.Count == 0)
        {
            result = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Where(e => e.Status == "active")
                .OrderByDescending(e => e.AvailableTickets)
                .Take(10)
                .ToListAsync();
        }
        else
        {
            result = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Where(e => topEventIds.Contains(e.Id) && e.Status == "active")
                .ToListAsync();

            result = topEventIds.Join(
                    result,
                    id => id,
                    ev => ev.Id,
                    (_, ev) => ev)
                .ToList();
        }

        return Ok(result);
    }

    [HttpPost("wizard-rank")]
    public async Task<ActionResult<WizardRecommendationResponseDto>> RankWizard(
        [FromBody] WizardRecommendationRequestDto? body,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _wizardRecommendationService.RankAsync(body ?? new WizardRecommendationRequestDto(), User.GetOptionalUserId(), cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка подбора", message = ex.Message });
        }
    }

    [HttpGet("recommended")]
    public async Task<ActionResult<IEnumerable<Event>>> GetRecommendedEvents([FromQuery] int limit = 6)
    {
        var recommendations = await _recommendationService.GetRecommendationsAsync(User.GetOptionalUserId(), limit);
        return Ok(recommendations);
    }

    [HttpGet("{id:int}/recommendations")]
    public async Task<ActionResult<IEnumerable<Event>>> GetRecommendations(int id)
    {
        var sourceEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (sourceEvent == null)
        {
            return NotFound(new { message = "РЎРѕР±С‹С‚РёРµ РЅРµ РЅР°Р№РґРµРЅРѕ" });
        }

        var recommendations = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.Id != id && e.Status == "active" && e.CategoryId == sourceEvent.CategoryId)
            .OrderBy(e => e.Id)
            .Take(6)
            .ToListAsync();

        return Ok(recommendations);
    }

    [HttpGet("{id:int}/seat-map")]
    public async Task<ActionResult<SeatMapDto>> GetSeatMap(int id)
    {
        var eventItem = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (eventItem == null)
            return NotFound();

        if (!SeatLayoutService.UsesAssignedSeats(eventItem))
        {
            return Ok(new SeatMapDto
            {
                SeatingType = eventItem.SeatingType,
                LayoutJson = "{}",
                Booked = new List<string>()
            });
        }

        var layoutJson = SeatLayoutService.ResolveLayoutJson(eventItem.SeatLayoutJson);
        var booked = (await _context.BookedSeats
                .AsNoTracking()
                .Where(b => b.EventId == id)
                .Select(b => b.SeatLabel)
                .ToListAsync())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var fromCarts = new List<string>();
        var cartJsons = await _context.CartItems
            .AsNoTracking()
            .Where(c => c.EventId == id && c.SelectedSeatsJson != null && c.SelectedSeatsJson != "")
            .Select(c => c.SelectedSeatsJson)
            .ToListAsync();
        foreach (var json in cartJsons)
        {
            try
            {
                var seats = JsonSerializer.Deserialize<List<string>>(json!);
                if (seats == null) continue;
                foreach (var s in seats)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                        fromCarts.Add(s.Trim());
                }
            }
            catch (JsonException)
            {
            }
        }

        var merged = booked
            .Concat(fromCarts.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return Ok(new SeatMapDto
        {
            SeatingType = eventItem.SeatingType,
            LayoutJson = layoutJson,
            Booked = merged
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(int id)
    {
        var eventItem = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        return eventItem;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent([FromBody] AdminEventUpsertDto dto)
    {
        try
        {
            var validationError = await ValidateAdminEventDtoAsync(dto);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            var eventItem = new Event
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                CategoryId = dto.CategoryId,
                VenueId = dto.VenueId,
                ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
                EventDate = NormalizeToUtc(dto.EventDate),
                EventTime = dto.EventTime,
                Price = dto.Price,
                AvailableTickets = dto.AvailableTickets,
                AgeRating = string.IsNullOrWhiteSpace(dto.AgeRating) ? "0+" : dto.AgeRating.Trim(),
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "active" : dto.Status.Trim().ToLowerInvariant(),
                SeatingType = string.IsNullOrWhiteSpace(dto.SeatingType) ? "general" : dto.SeatingType.Trim().ToLowerInvariant(),
                SeatLayoutJson = string.IsNullOrWhiteSpace(dto.SeatLayoutJson) ? null : dto.SeatLayoutJson.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            await _context.Entry(eventItem).Reference(e => e.Category).LoadAsync();
            await _context.Entry(eventItem).Reference(e => e.Venue).LoadAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при создании мероприятия", message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Event>> UpdateEvent(int id, [FromBody] AdminEventUpsertDto dto)
    {
        try
        {
            var eventItem = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (eventItem == null)
                return NotFound(new { message = "Событие не найдено" });

            var validationError = await ValidateAdminEventDtoAsync(dto);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            eventItem.Title = dto.Title.Trim();
            eventItem.Description = dto.Description.Trim();
            eventItem.CategoryId = dto.CategoryId;
            eventItem.VenueId = dto.VenueId;
            eventItem.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
            eventItem.EventDate = NormalizeToUtc(dto.EventDate);
            eventItem.EventTime = dto.EventTime;
            eventItem.Price = dto.Price;
            eventItem.AvailableTickets = dto.AvailableTickets;
            eventItem.AgeRating = string.IsNullOrWhiteSpace(dto.AgeRating) ? "0+" : dto.AgeRating.Trim();
            eventItem.Status = string.IsNullOrWhiteSpace(dto.Status) ? "active" : dto.Status.Trim().ToLowerInvariant();
            eventItem.SeatingType = string.IsNullOrWhiteSpace(dto.SeatingType) ? "general" : dto.SeatingType.Trim().ToLowerInvariant();
            eventItem.SeatLayoutJson = string.IsNullOrWhiteSpace(dto.SeatLayoutJson) ? null : dto.SeatLayoutJson.Trim();

            await _context.SaveChangesAsync();

            await _context.Entry(eventItem).Reference(e => e.Category).LoadAsync();
            await _context.Entry(eventItem).Reference(e => e.Venue).LoadAsync();
            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при обновлении мероприятия", message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        try
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
                return NotFound(new { message = "Событие не найдено" });

            var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.EventId == id);
            if (hasOrders)
            {
                return BadRequest(new
                {
                    message = "Нельзя удалить мероприятие: по нему уже есть оформленные заказы. Переведите его в статус inactive."
                });
            }

            var cartItems = _context.CartItems.Where(c => c.EventId == id);
            var favoriteItems = _context.FavoriteEvents.Where(f => f.EventId == id);
            var bookedSeats = _context.BookedSeats.Where(b => b.EventId == id);

            _context.CartItems.RemoveRange(cartItems);
            _context.FavoriteEvents.RemoveRange(favoriteItems);
            _context.BookedSeats.RemoveRange(bookedSeats);

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при удалении мероприятия", message = ex.Message });
        }
    }

    private async Task<string?> ValidateAdminEventDtoAsync(AdminEventUpsertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return "Укажите название мероприятия";
        if (string.IsNullOrWhiteSpace(dto.Description))
            return "Укажите описание мероприятия";
        if (dto.CategoryId <= 0 || !await _context.EventCategories.AnyAsync(c => c.Id == dto.CategoryId))
            return "Выбрана некорректная категория";
        if (dto.VenueId <= 0 || !await _context.Venues.AnyAsync(v => v.Id == dto.VenueId))
            return "Выбрана некорректная площадка";
        if (dto.Price < 0)
            return "Цена не может быть отрицательной";
        if (dto.AvailableTickets < 0)
            return "Количество билетов не может быть отрицательным";

        var seatingType = dto.SeatingType?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(seatingType) && seatingType != "general" && seatingType != "assigned")
            return "Тип рассадки должен быть general или assigned";

        if (seatingType == "assigned")
        {
            if (string.IsNullOrWhiteSpace(dto.SeatLayoutJson))
                return "Для assigned-рассадки заполните JSON-схему мест";

            try
            {
                using var doc = JsonDocument.Parse(dto.SeatLayoutJson);
                if (!doc.RootElement.TryGetProperty("rows", out var rows) || rows.ValueKind != JsonValueKind.Array)
                    return "JSON-схема мест должна содержать массив rows";

                var hasAnySeat = false;
                foreach (var row in rows.EnumerateArray())
                {
                    if (!row.TryGetProperty("seats", out var seats) || seats.ValueKind != JsonValueKind.Array)
                        continue;
                    if (seats.GetArrayLength() > 0)
                    {
                        hasAnySeat = true;
                        break;
                    }
                }

                if (!hasAnySeat)
                    return "JSON-схема мест должна содержать хотя бы одно место";
            }
            catch (JsonException)
            {
                return "JSON-схема мест содержит некорректный JSON";
            }
        }

        return null;
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

}

