using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/brands")]
public class BrandController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllProduct([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var values = await _mediator.Send(new GetAllBrandQuery(pageIndex, pageSize));
        return Ok(values);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddBrand([FromBody] AddBrandCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Thêm thành công!" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteBrand(IEnumerable<string> id)
    {
        await _mediator.Send(new DeleteBrandCommand(id));
        return Ok(new { message = "Xóa thành công." });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBrand([FromBody] UpdateBrandCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Cập nhật thành công." });
    }
}
