using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

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
    public async Task<IActionResult> GetAllCategories([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        try
        {
            var values = await _mediator.Send(new GetAllCategoryQuery(pageIndex, pageSize));
            return Ok(ApiResponse<object>.CreateSuccess(values, "Lấy danh sách danh mục thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CATEGORY_GET_ERROR"));
        }
    }

    [HttpGet("get_select")]
    public async Task<IActionResult> GetAllCategoriesForSelect()
    {
        try
        {
            var values = await _mediator.Send(new GetAllCategoryNamesQuery());
            return Ok(ApiResponse<object>.CreateSuccess(values, "Lấy danh sách tag thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CATEGORIES_GET_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpPost("add")]
    public async Task<IActionResult> AddCategory([FromBody] AddCategoryDto cateDto)
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
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var command = new AddCategoryCommand(
               cateDto.Code,
               cateDto.Name,
               username
           );
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm danh mục thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CATEGORY_ADD_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
    [Authorize(Roles = "Manager, Admin")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCategory(IEnumerable<string> id)
    {
        try
        {
            await _mediator.Send(new DeleteCategoryCommand(id));
            return Ok(ApiResponse<object>.CreateSuccess(null, "Xóa danh mục thành công!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CATEGORY_DELETE_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto cateDto)
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
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var command = new UpdateCategoryCommand(
               cateDto.Id,
               cateDto.Name,
               username
           );
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật danh mục thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "CATEGORY_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}