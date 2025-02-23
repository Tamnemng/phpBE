using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OMS.Core.Queries;

[ApiController]
[Route("[controller]")]
public class RedisController : ControllerBase
{
    private readonly IMediator _mediator;

    public RedisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetAllValues([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        var values = await _mediator.Send(new GetAllValuesQuery(pageIndex, pageSize));
        return Ok(values);
    }


    [HttpPost("set")]
    public async Task<IActionResult> SetValue([FromBody] KeyValuePair<string, string> data)
    {
        await _mediator.Send(new SetValueCommand(data.Key, data.Value));
        return Ok();
    }

    [HttpGet("get/{key}")]
    public async Task<IActionResult> GetValue(string key)
    {
        var value = await _mediator.Send(new GetValueQuery(key));
        if (value == null)
            return NotFound();
        return Ok(value);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateValue([FromBody] KeyValuePair<string, string> data)
    {
        try
        {
            await _mediator.Send(new UpdateValueCommand(data.Key, data.Value));
            return Ok("Cập nhật thành công.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> DeleteValue(string key)
    {
        await _mediator.Send(new DeleteValueCommand(key));
        return Ok();
    }
}