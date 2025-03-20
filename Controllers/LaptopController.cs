using MediatR;
using Microsoft.AspNetCore.Mvc;
using PHPBE.Models;

[ApiController]
[Route("api/laptops")]
public class LaptopController : ControllerBase
{
    private readonly IMediator _mediator;

    public LaptopController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLaptopById(string id)
    {
        var value = await _mediator.Send(new GetLaptopByIdQuery(id));
        return Ok(value);
    }

}
