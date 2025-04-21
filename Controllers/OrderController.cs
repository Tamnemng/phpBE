// Controllers/OrderController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var command = new CreateOrderCommand(userId, orderDto, username);
            var result = await _mediator.Send(command);

            return Ok(ApiResponse<OrderDetailDto>.CreateSuccess(result, "Đặt hàng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "ORDER_CREATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpGet("user")]
    public async Task<IActionResult> GetUserOrders([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var query = new GetUserOrdersQuery(userId, pageIndex, pageSize);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<object>.CreateSuccess(result, "Lấy danh sách đơn hàng thành công!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");

            // If user is not admin, only allow them to view their own orders
            var query = new GetOrderByIdQuery(id, isAdmin ? null : userId);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<OrderDetailDto>.CreateSuccess(result, "Lấy thông tin đơn hàng thành công!"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.Unauthorized, "UNAUTHORIZED"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "ORDER_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10, [FromQuery] OrderStatus? status = null)
    {
        try
        {
            var query = new GetAllOrdersQuery(pageIndex, pageSize, status);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<object>.CreateSuccess(result, "Lấy danh sách đơn hàng thành công!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("status")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var command = new UpdateOrderStatusCommand(updateDto.OrderId, updateDto.NewStatus, username);
            
            await _mediator.Send(command);
            
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật trạng thái đơn hàng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "ORDER_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("payment")]
    public async Task<IActionResult> UpdatePaymentStatus([FromBody] UpdatePaymentStatusDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var command = new UpdatePaymentStatusCommand(updateDto.OrderId, updateDto.NewStatus, username);
            
            await _mediator.Send(command);
            
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật trạng thái thanh toán thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PAYMENT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(string id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            // First check if order exists and belongs to user (if not admin)
            var getOrderQuery = new GetOrderByIdQuery(id, isAdmin ? null : userId);
            var order = await _mediator.Send(getOrderQuery);
            
            // Then cancel the order
            var command = new UpdateOrderStatusCommand(id, OrderStatus.Canceled, username);
            await _mediator.Send(command);
            
            return Ok(ApiResponse<object>.CreateSuccess(null, "Hủy đơn hàng thành công!"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.Unauthorized, "UNAUTHORIZED"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "ORDER_CANCEL_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}