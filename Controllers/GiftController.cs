using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

[ApiController]
[Route("api/gifts")]
public class GiftController : ControllerBase
{
    private readonly IMediator _mediator;

    public GiftController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get_select")]
    public async Task<IActionResult> GetAllGiftForSelect()
    {
        try
        {
            var values = await _mediator.Send(new GetAllGiftNamesQuery());
            return Ok(ApiResponse<object>.CreateSuccess(values, "Lấy danh sách quà tặng thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "GIFT_GET_ERROR"));
        }
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllGifts([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        try
        {
            var values = await _mediator.Send(new GetAllGiftQuery(pageIndex, pageSize));
            return Ok(ApiResponse<object>.CreateSuccess(values, "Lấy danh sách quà tặng thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "GIFT_GET_ERROR"));
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddBrand([FromBody] AddGiftCommand command)
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
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm thương quà tặng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "GIFT_ADD_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteBrand(IEnumerable<string> id)
    {
        try
        {
            await _mediator.Send(new DeleteGiftCommand(id));
            return Ok(ApiResponse<object>.CreateSuccess(null, "Xóa quà tặng thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_DELETE_ERROR"));
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBrand([FromBody] UpdateGiftCommand command)
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
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật quà tặng thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "GIFT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}