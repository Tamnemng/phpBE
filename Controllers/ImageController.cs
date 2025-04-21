using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/image-collections")]
public class ImageCollectionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;

    public ImageCollectionController(IMediator mediator, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetImageCollectionById(string id)
    {
        try
        {
            var collection = await _mediator.Send(new GetImageCollectionByIdQuery(id));
            return Ok(ApiResponse<ImageCollection>.CreateSuccess(collection, "Lấy thông tin bộ sưu tập ảnh thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "IMAGE_COLLECTION_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet("ids")]
    public async Task<IActionResult> GetAllImageCollectionIds()
    {
        try
        {
            var ids = await _mediator.Send(new GetAllImageCollectionIdsQuery());
            return Ok(ApiResponse<List<string>>.CreateSuccess(ids, "Lấy danh sách ID thành công!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpPost("add")]
    public async Task<IActionResult> AddImageCollection([FromBody] AddImageCollectionDto dto)
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
            
            // Upload tất cả các ảnh
            var uploadedImages = new List<ImageItem>();
            
            foreach (var imageDto in dto.Images)
            {
                if (!string.IsNullOrEmpty(imageDto.ImageBase64))
                {
                    string imageUrl = await _cloudinaryService.UploadImageBase64Async(imageDto.ImageBase64);
                    uploadedImages.Add(new ImageItem(imageUrl, imageDto.Priority));
                }
            }

            var command = new AddImageCollectionCommand(
                dto.Title,
                uploadedImages,
                username
            );

            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Thêm bộ sưu tập ảnh thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "IMAGE_COLLECTION_ADD_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteImageCollection(string id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteImageCollectionCommand(id));
            
            if (!result)
            {
                return NotFound(ApiResponse<object>.CreateError(
                    $"ImageCollection with ID {id} not found",
                    HttpStatusCode.NotFound,
                    "IMAGE_COLLECTION_NOT_FOUND"
                ));
            }
            
            return Ok(ApiResponse<object>.CreateSuccess(null, "Xóa bộ sưu tập ảnh thành công!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Manager, Admin")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateImageCollection([FromBody] UpdateImageCollectionDto dto)
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
            
            // Upload các ảnh mới nếu có
            var newUploadedImages = new List<ImageItem>();
            
            if (dto.NewImages != null)
            {
                foreach (var imageDto in dto.NewImages)
                {
                    if (!string.IsNullOrEmpty(imageDto.ImageBase64))
                    {
                        string imageUrl = await _cloudinaryService.UploadImageBase64Async(imageDto.ImageBase64);
                        newUploadedImages.Add(new ImageItem(imageUrl, imageDto.Priority));
                    }
                }
            }

            var command = new UpdateImageCollectionCommand(
                dto.Id,
                dto.Title,
                newUploadedImages,
                dto.UpdatedImages,
                dto.DeletedImageUrls,
                username
            );

            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Cập nhật bộ sưu tập ảnh thành công!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "IMAGE_COLLECTION_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}