using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/gifts")]
public class GiftController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;

    public GiftController(IMediator mediator, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
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

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("add")]
    public async Task<IActionResult> AddGift([FromBody] AddGiftDto giftDto)
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
            // Upload image to Cloudinary if base64 is provided
            string imageUrl = null;
            if (!string.IsNullOrEmpty(giftDto.ImageBase64))
            {
                imageUrl = await _cloudinaryService.UploadImageBase64Async(giftDto.ImageBase64);
            }
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            // Create command with image URL from Cloudinary
            var command = new AddGiftCommand(
                giftDto.Code,
                giftDto.Name,
                imageUrl ?? "", // Use uploaded URL or empty string
                username
            );

            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm quà tặng thành công!"));
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

    [Authorize(Roles = "Admin, Manager")]
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

    [Authorize(Roles = "Admin, Manager")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateGift([FromBody] UpdateGiftDto giftDto)
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
            // Upload image to Cloudinary if base64 is provided
            string imageUrl = null;
            if (!string.IsNullOrEmpty(giftDto.ImageBase64))
            {
                imageUrl = await _cloudinaryService.UploadImageBase64Async(giftDto.ImageBase64);
            }
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            var command = new UpdateGiftCommand(
                giftDto.Code,
                giftDto.Name,
                imageUrl ?? "",
                username
            );
            
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