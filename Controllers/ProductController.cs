using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OMS.Core.Queries;
using OMS.Core.Utilities;
using Think4.Services;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductController(IMediator mediator, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("add")]
    public async Task<IActionResult> AddProduct([FromBody] ProductCreateDto productDto)
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
            if (productDto.Variants == null || !productDto.Variants.Any() ||
                productDto.Variants.Any(v => v.Options == null || !v.Options.Any()))
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "At least one variant group with options is required",
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR"
                ));
            }

            // Check if there are any image-requiring variants (e.g., color variants)
            bool hasImageVariant = false;
            foreach (var group in productDto.Variants)
            {
                // Check if this is an image-requiring variant (like color)
                bool isImageVariant = IsImageRequiringVariant(group.OptionTitle);

                if (isImageVariant)
                {
                    hasImageVariant = true;

                    // Verify all options in image-requiring variant groups have images
                    foreach (var option in group.Options)
                    {
                        if (option.ImagesBase64 == null || !option.ImagesBase64.Any())
                        {
                            return BadRequest(ApiResponse<object>.CreateError(
                                $"All '{group.OptionTitle}' variants must have at least one Base64 image",
                                HttpStatusCode.BadRequest,
                                "VALIDATION_ERROR"
                            ));
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(productDto.ImageBase64))
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Product main image in Base64 format is required",
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR"
                ));
            }

            productDto.ImageBase64 = await _cloudinaryService.UploadImageBase64Async(productDto.ImageBase64);
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var command = new AddProductCommand(productDto, username);

            command.ImageUrl = productDto.ImageBase64;

            await _mediator.Send(command);

            return Ok(ApiResponse<object>.CreateSuccess(null, "Product added successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_INVALID"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    private bool IsImageRequiringVariant(string variantTitle)
    {

        string title = variantTitle.ToLowerInvariant();
        return title.Contains("color") || title.Contains("Color") || title.Contains("Màu") || title.Contains("màu") || title.Contains("colour") || title.Contains("Colour");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        try
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query);

            return Ok(ApiResponse<Product>.CreateSuccess(product, "Retrieved product information successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? productName = null, [FromQuery] string? brandCode = null, [FromQuery] string? categoryCode = null, [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetAllProductsQuery(
                productName,
                brandCode,
                categoryCode,
                pageIndex,
                pageSize);
            var pagedProducts = await _mediator.Send(query);

            return Ok(ApiResponse<PagedModel<ProductSummaryDto>>.CreateSuccess(pagedProducts, "Retrieved product list successfully!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
    [Authorize(Roles = "Admin, Manager")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteBrand(IEnumerable<string> code)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand(code));
            return Ok(ApiResponse<object>.CreateSuccess(null, "Products deleted successfully!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_DELETE_ERROR"));
        }
    }


    [HttpGet("related")]
    public async Task<IActionResult> GetRelatedProducts([FromQuery] string productCode, [FromQuery] int count = 5)
    {
        if (string.IsNullOrEmpty(productCode))
        {
            return BadRequest(ApiResponse<object>.CreateError(
                "Product code is required",
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var query = new GetRelatedProductsQuery(productCode, count);
            var relatedProducts = await _mediator.Send(query);

            return Ok(ApiResponse<List<ProductSummaryDto>>.CreateSuccess(relatedProducts, "Retrieved related products list successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet("{code}/variants")]
    public async Task<IActionResult> GetProductVariants(string code)
    {
        try
        {
            // Create query to get all products with the same code
            var query = new GetProductVariantsQuery(code);
            var variants = await _mediator.Send(query);

            return Ok(ApiResponse<ProductVariantsDto>.CreateSuccess(variants, "Retrieved product variants successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpGet("getUpdateTemplate/{code}")]
    [ProducesResponseType(typeof(ProductCreateDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetProductTemplate(string code)
    {
        try
        {
            var query = new GetProductTemplateQuery(code);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPut("updateProductBrand")]
    public async Task<IActionResult> UpdateProductBrand(string productCode ,[FromBody] UpdateProductBrandDto updateProductBrandDto)
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
            var command = new UpdateProductBrandCommand(productCode, updateProductBrandDto.BrandCode, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Brand updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductCategories")]
    public async Task<IActionResult> UpdateProductCategories(string productCode ,[FromBody] UpdateProductCategoriesDto updateProductCategoriesDto)
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
            var command = new UpdateProductCategoriesCommand(productCode, updateProductCategoriesDto.CategoriesCode, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product categories updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductMainImage")]
    public async Task<IActionResult> UpdateProductMainImage(string productCode ,[FromBody] UpdateProductImageDto updateProductImageDto)
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
            if (string.IsNullOrEmpty(updateProductImageDto.ImageBase64))
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Product main image in Base64 format or URL is required",
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR"
                ));
            }

            string urlImage;
            if (updateProductImageDto.ImageBase64.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                urlImage = updateProductImageDto.ImageBase64;
            }
            else if (updateProductImageDto.ImageBase64.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                urlImage = await _cloudinaryService.UploadImageBase64Async(updateProductImageDto.ImageBase64);
            }
            else
            {
                return BadRequest(ApiResponse<object>.CreateError(
                    "Invalid image format. Please provide a valid Base64 image string or a URL.",
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR"
                ));
            }
            var command = new UpdateProductImageCommand(productCode, urlImage, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product image updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductStatusById")]
    public async Task<IActionResult> UpdateProductStatusById(string productId, [FromBody] UpdateProductStatusDto updateProductStatusDto)
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
            var command = new UpdateProductStatusByIdCommand(productId, updateProductStatusDto.Status, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product status updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductStatusByCode")]
    public async Task<IActionResult> UpdateProductStatusByCode(string productCode, [FromBody] UpdateProductStatusDto updateProductStatusDto)
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
            var command = new UpdateProductStatusByCodeCommand(productCode, updateProductStatusDto.Status, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product status updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductPriceById")]
    public async Task<IActionResult> UpdateVariantPrice(string productId, [FromBody] UpdateVariantPriceDto updateVariantPriceDto)
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
            var command = new UpdateVariantPriceCommand(productId, updateVariantPriceDto.OriginalPrice, updateVariantPriceDto.CurrentPrice, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product price updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductName")]
    public async Task<IActionResult> UpdateProductName(string productCode, [FromBody] UpdateProductNameDto updateProductNameDto)
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
            var command = new UpdateProductNameCommand(productCode, updateProductNameDto.Name, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product name updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductGift")]
    public async Task<IActionResult> UpdateProductGift(string productCode, [FromBody] UpdateProductGiftsDto updateProductGiftDto)
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
            var command = new UpdateProductGiftsCommand(productCode, updateProductGiftDto.GiftCodes, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product gifts updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [HttpPut("updateProductDescriptions")]
    public async Task<IActionResult> UpdateProductDescriptions(string productId, [FromBody] UpdateVariantDescriptionsDto updateVariantDescriptionsDto)
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
            var command = new UpdateProductDescriptionsCommand(productId, updateVariantDescriptionsDto.Descriptions, updateVariantDescriptionsDto.ShortDescription, username);
            await _mediator.Send(command);
            return Ok(ApiResponse<object>.CreateSuccess(null, "Product descriptions updated successfully!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "PRODUCT_UPDATE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}