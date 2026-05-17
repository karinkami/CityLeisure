using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Mapping;
using CityLeisure.Api.Models;
using CityLeisure.Api.Security;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/cart-items")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
    {
        var userId = User.GetRequiredUserId();

        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Event)
            .ThenInclude(e => e!.Category)
            .Include(c => c.Event)
            .ThenInclude(e => e!.Venue)
            .ToListAsync();

        return Ok(cartItems.Select(MapCartItem).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> AddToCart([FromBody] AddToCartDto dto)
    {
        var userId = User.GetRequiredUserId();

        var eventItem = await _context.Events.FindAsync(dto.EventId);
        if (eventItem == null)
        {
            return NotFound(new { message = "Мероприятие не найдено" });
        }

        if (dto.Quantity < 1)
        {
            return BadRequest(new { message = "Количество билетов должно быть больше 0" });
        }

        if (dto.Quantity > eventItem.AvailableTickets)
        {
            return BadRequest(new { message = "Недостаточно доступных билетов" });
        }

        var err = ValidateSeatsForEvent(eventItem, dto);
        if (err != null)
            return BadRequest(new { message = err });

        var bookedConflict = await SeatsAlreadyBooked(dto.EventId, dto.Seats);
        if (bookedConflict.Count > 0)
        {
            return BadRequest(new { message = $"Места уже заняты: {string.Join(", ", bookedConflict)}" });
        }

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.EventId == dto.EventId);

        if (SeatLayoutService.UsesAssignedSeats(eventItem))
        {
            if (existingItem != null)
            {
                existingItem.Quantity = dto.Quantity;
                existingItem.SelectedSeatsJson = SerializeSeats(dto.Seats);
            }
            else
            {
                existingItem = new CartItem
                {
                    UserId = userId,
                    EventId = dto.EventId,
                    Quantity = dto.Quantity,
                    SelectedSeatsJson = SerializeSeats(dto.Seats)
                };
                _context.CartItems.Add(existingItem);
            }
        }
        else
        {
            if (dto.Seats is { Count: > 0 })
                return BadRequest(new { message = "Для этого мероприятия выбор места не требуется" });

            if (existingItem != null)
            {
                if (existingItem.Quantity + dto.Quantity > eventItem.AvailableTickets)
                    return BadRequest(new { message = "Недостаточно доступных билетов" });
                existingItem.Quantity += dto.Quantity;
                existingItem.SelectedSeatsJson = null;
            }
            else
            {
                existingItem = new CartItem
                {
                    UserId = userId,
                    EventId = dto.EventId,
                    Quantity = dto.Quantity,
                    SelectedSeatsJson = null
                };
                _context.CartItems.Add(existingItem);
            }
        }

        await _context.SaveChangesAsync();

        await _context.Entry(existingItem)
            .Reference(c => c.Event)
            .LoadAsync();

        if (existingItem.Event != null)
        {
            await _context.Entry(existingItem.Event).Reference(e => e.Category).LoadAsync();
            await _context.Entry(existingItem.Event).Reference(e => e.Venue).LoadAsync();
        }

        return Ok(MapCartItem(existingItem));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCartItem(int id, [FromBody] AddToCartDto dto)
    {
        var userId = User.GetRequiredUserId();

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new { message = "Позиция корзины не найдена" });
        }

        if (dto.Quantity < 1)
        {
            return BadRequest(new { message = "Количество билетов должно быть больше 0" });
        }

        var eventItem = await _context.Events.FindAsync(cartItem.EventId);
        if (eventItem == null)
        {
            return BadRequest(new { message = "Мероприятие недоступно" });
        }

        if (dto.Quantity > eventItem.AvailableTickets)
        {
            return BadRequest(new { message = "Недостаточно доступных билетов" });
        }

        var err = ValidateSeatsForEvent(eventItem, dto);
        if (err != null)
            return BadRequest(new { message = err });

        var bookedConflict = await SeatsAlreadyBooked(cartItem.EventId, dto.Seats, ignoreCartItemId: cartItem.Id);
        if (bookedConflict.Count > 0)
        {
            return BadRequest(new { message = $"Места уже заняты: {string.Join(", ", bookedConflict)}" });
        }

        cartItem.Quantity = dto.Quantity;
        cartItem.SelectedSeatsJson = SeatLayoutService.UsesAssignedSeats(eventItem)
            ? SerializeSeats(dto.Seats)
            : null;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidateSeatsForEvent(Event eventItem, AddToCartDto dto)
    {
        if (SeatLayoutService.UsesAssignedSeats(eventItem))
        {
            if (dto.Seats == null || dto.Seats.Count == 0)
                return "Выберите места на схеме зала";
            if (dto.Seats.Count != dto.Quantity)
                return "Количество билетов должно совпадать с числом выбранных мест";
            if (dto.Seats.Count != dto.Seats.Distinct(StringComparer.Ordinal).Count())
                return "Нельзя выбрать одно место дважды";

            var layout = SeatLayoutService.ResolveLayoutJson(eventItem.SeatLayoutJson);
            var valid = SeatLayoutService.ParseValidSeatLabels(layout);
            foreach (var s in dto.Seats)
            {
                if (!valid.Contains(s))
                    return $"Недопустимое место: {s}";
            }
        }
        else if (dto.Seats is { Count: > 0 })
            return "Для этого мероприятия выбор места не требуется";

        return null;
    }

    private async Task<List<string>> SeatsAlreadyBooked(int eventId, List<string>? seats, int? ignoreCartItemId = null)
    {
        if (seats == null || seats.Count == 0)
            return new List<string>();

        var bookedInOrders = await _context.BookedSeats
            .Where(b => b.EventId == eventId && seats.Contains(b.SeatLabel))
            .Select(b => b.SeatLabel)
            .ToListAsync();

        var cartQuery = _context.CartItems.Where(c => c.EventId == eventId && c.SelectedSeatsJson != null);
        if (ignoreCartItemId.HasValue)
            cartQuery = cartQuery.Where(c => c.Id != ignoreCartItemId.Value);

        var fromOthersCarts = new List<string>();
        foreach (var c in await cartQuery.ToListAsync())
        {
            var parsed = DeserializeSeats(c.SelectedSeatsJson);
            if (parsed == null) continue;
            foreach (var s in parsed.Where(seats.Contains))
                fromOthersCarts.Add(s);
        }

        return bookedInOrders.Concat(fromOthersCarts).Distinct().ToList();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> RemoveFromCart(int id)
    {
        var userId = User.GetRequiredUserId();

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (cartItem == null)
        {
            return NotFound(new { message = "Позиция корзины не найдена" });
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart()
    {
        var userId = User.GetRequiredUserId();

        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static CartItemDto MapCartItem(CartItem c)
    {
        return new CartItemDto
        {
            Id = c.Id,
            EventId = c.EventId,
            Quantity = c.Quantity,
            SelectedSeats = DeserializeSeats(c.SelectedSeatsJson),
            Event = c.Event != null ? MapEvent(c.Event) : null
        };
    }

    private static EventDto MapEvent(Event e) => EventDtoMapper.FromEvent(e);

    private static string? SerializeSeats(List<string>? seats) =>
        seats == null || seats.Count == 0 ? null : JsonSerializer.Serialize(seats);

    private static List<string>? DeserializeSeats(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json);
        }
        catch
        {
            return null;
        }
    }
}
