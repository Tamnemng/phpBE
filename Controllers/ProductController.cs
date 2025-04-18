using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OMS.Core.Queries;
using OMS.Core.Utilities;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddProduct([FromBody] ProductCreateDto productDto)
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
            // Validate that we have at least one variant group with options
            if (productDto.Variants == null || !productDto.Variants.Any() || 
                productDto.Variants.Any(v => v.Options == null || !v.Options.Any()))
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "At least one variant group with options is required",
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR"
                ));
            }

            // Convert from DTO to Command
            var command = new AddProductCommand(productDto);

            // Send command to handler
            await _mediator.Send(command);

            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm sản phẩm thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_INVALID"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        try
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query);

            return Ok(ApiResponse<Product>.CreateSuccess(product, "Lấy thông tin sản phẩm thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? productName = null, [FromQuery] string? brandCode = null, [FromQuery] string? categoryCode = null, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetAllProductsQuery(
                productName,
                brandCode,
                categoryCode,
                pageIndex,
                pageSize);
            var pagedProducts = await _mediator.Send(query);

            return Ok(ApiResponse<PagedModel<ProductSummaryDto>>.CreateSuccess(pagedProducts, "Lấy danh sách sản phẩm thành công!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteBrand(IEnumerable<string> code)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand(code));
            return Ok(ApiResponse<object>.CreateSuccess(null, "Xóa sản phẩm thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_DELETE_ERROR"));
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateDto productDto)
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
            // Convert from DTO to Command
            var command = new UpdateProductCommand(productDto);

            // Send command to handler
            await _mediator.Send(command);

            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật sản phẩm thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}