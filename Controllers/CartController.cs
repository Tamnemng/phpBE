using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

[ApiController]
[Route("api/Carts")]
public class CartController : ControllerBase{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartCommand command)
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
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm vào giỏ hàng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CART_ADD_ERROR"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "SERVER_ERROR"));
        }
    }

    [HttpGet("get/{userId}")]
    public async Task<IActionResult> GetCart(string userId)
    {
        try
        {
            var cart = await _mediator.Send(new GetCartQuery(userId));
            return Ok(ApiResponse<object>.CreateSuccess(cart, "Lấy giỏ hàng thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "SERVER_ERROR"));
        }
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFromCart([FromBody] DeleteFromCartCommand command)
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
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Xóa sản phẩm khỏi giỏ hàng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CART_DELETE_ERROR"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "SERVER_ERROR"));
        }
    }

}