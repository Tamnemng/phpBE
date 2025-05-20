using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;

    public BrandController(IMediator mediator, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet("get_select")]
    public async Task<IActionResult> GetAllBrandForSelect()
    {
        try
        {
            var values = await _mediator.Send(new GetAllBrandNamesQuery());
            return Ok(ApiResponse<object>.CreateSuccess(values, "Brand list retrieved successfully!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_GET_ERROR"));
        }
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllBrands([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        try
        {
            var values = await _mediator.Send(new GetAllBrandQuery(pageIndex, pageSize));
            return Ok(ApiResponse<object>.CreateSuccess(values, "Brand list retrieved successfully!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_GET_ERROR"));
        }
    }
    [Authorize(Roles = "Manager, Admin")]
    [HttpPost("add")]
    public async Task<IActionResult> AddGift([FromBody] AddBrandDto giftDto)
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
            string imageUrl = null;
            if (!string.IsNullOrEmpty(giftDto.ImageBase64))
            {
                imageUrl = await _cloudinaryService.UploadImageBase64Async(giftDto.ImageBase64);
            }

            var command = new AddBrandCommand(
                giftDto.Code,
                giftDto.Name,
                imageUrl ?? "", // Use uploaded URL or empty string
                username
            );

            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Brand added successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_ADD_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteBrand(IEnumerable<string> id)
    {
        try
        {
            await _mediator.Send(new DeleteBrandCommand(id));
            return Ok(ApiResponse<object>.CreateSuccess(null, "Brand deleted successfully!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_DELETE_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateGift([FromBody] UpdateBrandDto giftDto)
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
            string imageUrl = null;

            if (!string.IsNullOrEmpty(giftDto.ImageBase64) &&
                !giftDto.ImageBase64.StartsWith("http"))
            {
                imageUrl = await _cloudinaryService.UploadImageBase64Async(giftDto.ImageBase64);
            }

            var command = new UpdateBrandCommand(
                giftDto.Id,
                giftDto.Name,
                imageUrl,
                username
            );

            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Brand updated successfully!"));
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