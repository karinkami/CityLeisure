using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CityLeisure.Api.DTOs;
using CityLeisure.Api.Security;
using CityLeisure.Api.Services;

namespace CityLeisure.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderCreatedDto>> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        _ = orderDto;
        var userId = User.GetRequiredUserId();
        if (userId == 0)
        {
            return Unauthorized(new { message = "Не удалось определить пользователя" });
        }

        try
        {
            var result = await _orderService.CreateOrderAsync(userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при создании заказа", message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
    {
        try
        {
            var userId = User.GetRequiredUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Не удалось определить пользователя" });
            }

            var result = await _orderService.GetUserOrdersAsync(userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка при загрузке заказов", message = ex.Message });
        }
    }
}
