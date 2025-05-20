using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

[ApiController]
[Route("api/carts")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddCartDto dto)
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
            var command = new AddToCartCommand(
               userId,
               dto.ItemId,
               dto.ItemType,
               dto.Quantity
           );
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

    [Authorize]
    [HttpGet("get")]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var cart = await _mediator.Send(new GetCartQuery(userId));
            return Ok(ApiResponse<object>.CreateSuccess(cart, "Lấy giỏ hàng thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpGet("get/{userId}")]
    public async Task<IActionResult> GetUserCart(string userId)
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

    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteFromCart([FromBody] DeleteCartDto dto)
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
            var command = new DeleteFromCartCommand(
                userId,
                dto.ItemInfo.Select(item => 
                    new CartItemInfo(item.ItemId, item.ItemType)).ToArray()
            );

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

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateCart([FromBody] UpdateCartDto dto)
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
            var command = new UpdateCartCommand(
               userId,
               dto.OldItemId,
               dto.OldItemType,
               dto.NewItemId,
               dto.NewItemType,
               dto.Quantity
           );
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật giỏ hàng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CART_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "SERVER_ERROR"));
        }
    }
    
    [HttpPost("get-items-details")]
    public async Task<IActionResult> GetItemsDetails([FromBody] List<ItemRequest> items)
    {
        try
        {
            var itemsDetails = await _mediator.Send(new GetItemsDetailsQuery(items));
            return Ok(ApiResponse<object>.CreateSuccess(itemsDetails, "Lấy thông tin sản phẩm thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "SERVER_ERROR"));
        }
    }
}