using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class RedisController : ControllerBase
{
    private readonly IMediator _mediator;

    public RedisController(IMediator mediator)
    {
        _mediator = mediator;
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

    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> DeleteValue(string key)
    {
        await _mediator.Send(new DeleteValueCommand(key));
        return Ok();
    }
}