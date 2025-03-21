using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/laptops")]
public class LaptopController : ControllerBase
{
    private readonly IMediator _mediator;

    public LaptopController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetLaptopById(string id)
    {
        var value = await _mediator.Send(new GetLaptopByIdQuery(id));
        if (value == null)
            return NotFound();
        return Ok(value);
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllLaptops([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var values = await _mediator.Send(new GetAllLaptopQuery(pageIndex, pageSize));
        return Ok(values);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddLaptop([FromBody] AddLaptopCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Laptop đã được thêm thành công!" });
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteLaptop(string id)
    {
        await _mediator.Send(new DeleteLaptopCommand(id));
        return Ok("Xóa thành công.");
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateLaptop([FromBody] UpdateLaptopCommand command)
    {
        await _mediator.Send(command);
        return Ok("Cập nhật thành công.");
    }

}
