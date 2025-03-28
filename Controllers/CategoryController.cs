using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get")]
    public async Task<IActionResult> getAllCategories([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var values = await _mediator.Send(new GetAllCategoryQuery(pageIndex, pageSize));
        return Ok(values);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddCategory([FromBody] AddCategoryCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Thêm thành công!" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteCategory(IEnumerable<string> id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(new { message = "Xóa thành công." });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Cập nhật thành công." });
    }
}
