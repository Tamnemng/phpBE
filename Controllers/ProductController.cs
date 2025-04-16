using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // [HttpGet("get")]
    // public async Task<IActionResult> GetAllProduct([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    // {
    //     var values = await _mediator.Send(new GetAllBrandQuery(pageIndex, pageSize));
    //     return Ok(values);
    // }

    [HttpPost("add")]
    public async Task<IActionResult> AddProduct([FromBody] AddProductCommand command)
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

    // [HttpDelete("delete/{id}")]
    // public async Task<IActionResult> DeleteBrand(IEnumerable<string> id)
    // {
    //     await _mediator.Send(new DeleteBrandCommand(id));
    //     return Ok(new { message = "Xóa thành công." });
    // }

    // [HttpPut("update")]
    // public async Task<IActionResult> UpdateBrand([FromBody] UpdateBrandCommand command)
    // {
    //     await _mediator.Send(command);
    //     return Ok(new { message = "Cập nhật thành công." });
    // }
}
