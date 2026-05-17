using CityLeisure.Api.DTOs;

namespace CityLeisure.Api.Services;

public interface IOrderService
{
    Task<OrderCreateResult> CreateOrderAsync(int userId);
    Task<OrderListResult> GetUserOrdersAsync(int userId);
}

public sealed class OrderCreateResult
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public OrderCreatedDto? Data { get; init; }
}

public sealed class OrderListResult
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public List<OrderDto>? Data { get; init; }
}
