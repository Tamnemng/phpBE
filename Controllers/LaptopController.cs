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
    public async Task<IActionResult> UpdateLaptop([FromBody] KeyValuePair<string, string> data)
    {
        try
        {
            await _mediator.Send(new UpdateLaptopCommand(data.Key, data.Value));
            return Ok("Cập nhật thành công.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllLaptops([FromBody] int pageIndex = 0, [FromBody] int pageSize = 10)
    {
        var values = await _mediator.Send(new GetAllLaptopQuery(pageIndex, pageSize));
        return Ok(values);
    }
}
