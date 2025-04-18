using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get_select")]
    public async Task<IActionResult> GetAllBrandForSelect()
    {
        try
        {
            var values = await _mediator.Send(new GetAllBrandNamesQuery());
            return Ok(ApiResponse<object>.CreateSuccess(values, "Lấy danh sách hãng thành công!"));
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
            return Ok(ApiResponse<object>.CreateSuccess(values, "Lấy danh sách thương hiệu thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_GET_ERROR"));
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddBrand([FromBody] AddBrandCommand command)
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
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm thương hiệu thành công!"));
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

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteBrand(IEnumerable<string> id)
    {
        try
        {
            await _mediator.Send(new DeleteBrandCommand(id));
            return Ok(ApiResponse<object>.CreateSuccess(null, "Xóa thương hiệu thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_DELETE_ERROR"));
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBrand([FromBody] UpdateBrandCommand command)
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
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật thương hiệu thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "BRAND_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}