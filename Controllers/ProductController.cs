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

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetLaptopById(string id)
    {
        var value = await _mediator.Send(new GetProductByIdQuery(id));
        if (value == null)
            return NotFound();
        return Ok(value);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllLaptops([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var values = await _mediator.Send(new GetAllProductQuery(pageIndex, pageSize));
        return Ok(values);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddLaptop([FromBody] AddProductCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Thêm thành công!" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteLaptop(string id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return Ok(new { message = "Xóa thành công." });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateLaptop([FromBody] UpdateProductCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Cập nhật thành công." });
    }

    [HttpGet("get-by-category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(string categoryId)
    {
        var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId));
        return Ok(products);
    }
}
