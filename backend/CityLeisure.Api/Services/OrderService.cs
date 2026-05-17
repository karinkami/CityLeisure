using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CityLeisure.Api.Data;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Mapping;
using CityLeisure.Api.Models;

namespace CityLeisure.Api.Services;

public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderCreateResult> CreateOrderAsync(int userId)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Event)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                await tx.RollbackAsync();
                return new OrderCreateResult
                {
                    StatusCode = 400,
                    ErrorMessage = "Корзина пуста"
                };
            }

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Event == null)
                {
                    await tx.RollbackAsync();
                    return new OrderCreateResult
                    {
                        StatusCode = 400,
                        ErrorMessage = "Одно из мероприятий недоступно"
                    };
                }

                if (cartItem.Quantity > cartItem.Event.AvailableTickets)
                {
                    await tx.RollbackAsync();
                    return new OrderCreateResult
                    {
                        StatusCode = 400,
                        ErrorMessage = $"Недостаточно билетов для мероприятия \"{cartItem.Event.Title}\""
                    };
                }

                if (SeatLayoutService.UsesAssignedSeats(cartItem.Event))
                {
                    var seats = DeserializeSeatList(cartItem.SelectedSeatsJson);
                    if (seats == null || seats.Count != cartItem.Quantity || seats.Count != seats.Distinct(StringComparer.Ordinal).Count())
                    {
                        await tx.RollbackAsync();
                        return new OrderCreateResult
                        {
                            StatusCode = 400,
                            ErrorMessage = $"Для «{cartItem.Event.Title}» выберите места на схеме и повторите оформление."
                        };
                    }

                    var taken = await _context.BookedSeats
                        .Where(b => b.EventId == cartItem.EventId && seats.Contains(b.SeatLabel))
                        .Select(b => b.SeatLabel)
                        .ToListAsync();
                    if (taken.Count > 0)
                    {
                        await tx.RollbackAsync();
                        return new OrderCreateResult
                        {
                            StatusCode = 400,
                            ErrorMessage = $"Места уже проданы: {string.Join(", ", taken)}. Обновите корзину."
                        };
                    }
                }
            }

            var totalAmount = cartItems.Sum(item => item.Event!.Price * item.Quantity);

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    EventId = cartItem.EventId,
                    Price = cartItem.Event!.Price,
                    Quantity = cartItem.Quantity,
                    SeatLabelsJson = cartItem.SelectedSeatsJson
                };
                _context.OrderItems.Add(orderItem);

                if (!string.IsNullOrWhiteSpace(cartItem.SelectedSeatsJson))
                {
                    var labels = DeserializeSeatList(cartItem.SelectedSeatsJson) ?? new List<string>();
                    foreach (var label in labels)
                    {
                        _context.BookedSeats.Add(new BookedSeat
                        {
                            EventId = cartItem.EventId,
                            SeatLabel = label,
                            OrderId = order.Id
                        });
                    }
                }

                cartItem.Event.AvailableTickets -= cartItem.Quantity;
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            var createdOrder = await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (createdOrder == null)
            {
                return new OrderCreateResult
                {
                    StatusCode = 500,
                    ErrorMessage = "Ошибка при создании заказа"
                };
            }

            return new OrderCreateResult
            {
                IsSuccess = true,
                StatusCode = 200,
                Data = new OrderCreatedDto
                {
                    Id = createdOrder.Id,
                    TotalAmount = createdOrder.TotalAmount,
                    Status = createdOrder.Status,
                    CreatedAt = createdOrder.CreatedAt
                }
            };
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderListResult> GetUserOrdersAsync(int userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Event)
                    .ThenInclude(e => e!.Category)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Event)
                    .ThenInclude(e => e!.Venue)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var orderDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            OrderItems = o.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                EventId = oi.EventId,
                Price = oi.Price,
                Quantity = oi.Quantity,
                SeatLabels = DeserializeSeatList(oi.SeatLabelsJson),
                Event = oi.Event != null ? EventDtoMapper.FromEvent(oi.Event) : null
            }).ToList()
        }).ToList();

        return new OrderListResult
        {
            IsSuccess = true,
            StatusCode = 200,
            Data = orderDtos
        };
    }

    private static List<string>? DeserializeSeatList(string? json)
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
