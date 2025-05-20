using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;

[ApiController]
[Route("api/combos")]
public class ComboController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;

    public ComboController(IMediator mediator, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCombos([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new GetAllCombosQuery(pageIndex, pageSize, includeInactive);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<object>.CreateSuccess(result, "Combo list retrieved successfully!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetComboById(string id)
    {
        try
        {
            var query = new GetComboByIdQuery(id);
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<ComboDto>.CreateSuccess(result, "Combo details retrieved successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "COMBO_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("add")]
    public async Task<IActionResult> AddCombo([FromBody] AddComboDto comboDto)
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
            string imageUrl = null;
            if (!string.IsNullOrEmpty(comboDto.ImageBase64))
            {
                imageUrl = await _cloudinaryService.UploadImageBase64Async(comboDto.ImageBase64);
            }

            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var command = new AddComboCommand(
                comboDto.Name,
                comboDto.Description,
                imageUrl ?? "",
                comboDto.ProductCodes,
                comboDto.ComboPrice,
                username
            );

            await _mediator.Send(command);

            return Ok(ApiResponse<object>.CreateSuccess(null, "Combo added successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "COMBO_ADD_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateCombo([FromBody] UpdateComboDto comboDto)
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
            string imageUrl = null;
            if (!string.IsNullOrEmpty(comboDto.ImageBase64))
            {
                imageUrl = await _cloudinaryService.UploadImageBase64Async(comboDto.ImageBase64);
            }

            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var command = new UpdateComboCommand(
                comboDto.Id,
                comboDto.Name,
                comboDto.Description,
                imageUrl,
                comboDto.ProductCodes,
                comboDto.ComboPrice,
                comboDto.IsActive,
                username
            );

            await _mediator.Send(command);

            return Ok(ApiResponse<object>.CreateSuccess(null, "Combo updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "COMBO_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteCombo(string id)
    {
        try
        {
            var command = new DeleteComboCommand(id);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(ApiResponse<object>.CreateError(
                    $"Combo with ID {id} not found",
                    HttpStatusCode.NotFound,
                    "COMBO_NOT_FOUND"
                ));
            }

            return Ok(ApiResponse<object>.CreateSuccess(null, "Combo deleted successfully!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet("by-product/{productCode}")]
    public async Task<IActionResult> GetCombosByProductCode(string productCode)
    {
        try
        {
            var query = new GetCombosByProductCodeQuery(productCode);
            var result = await _mediator.Send(query);
            
            return Ok(ApiResponse<List<ComboDto>>.CreateSuccess(result, "Retrieved combo list by product successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "COMBO_PRODUCT_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}