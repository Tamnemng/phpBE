using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Think4.Services; // Namespace của ICloudinaryService và có thể cả ApiResponse
// Giả sử các DTOs, Commands, và Queries nằm trong các namespace này:
// Điều chỉnh lại nếu cần thiết dựa trên cấu trúc thực tế của dự án bạn.
// using Think4.Queries; // Cho ApiResponse nếu nó ở đây
// using Think4.Models; // Cho Brand, PagedModel nếu cần trực tiếp
// using Think4.Queries.Brand; // Cho AddBrandDto, UpdateBrandDto, Commands, Queries liên quan đến Brand
// using Think4.Handlers.Brand; // Cho BrandNameCodeDto nếu nó được định nghĩa ở đó

// Vì BrandNameCodeDto được định nghĩa trong Handlers/Brand/GetAllBrandHandler.cs
// và PagedModel trong Models/PageModel.cs (OMS.Core.Queries)
// Bạn cần đảm bảo các using này đúng
using OMS.Core.Queries; // Cho PagedModel
// using Think4.Models.Brand; // Cho Brand model nếu PagedModel<Brand> được sử dụng
// using Think4.Queries.Brand; // (đã có ở trên)
// using Think4.Handlers.Brand; // (đã có ở trên)


public class BrandControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ICloudinaryService> _mockCloudinaryService;
    private readonly BrandController _controller;

    public BrandControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockCloudinaryService = new Mock<ICloudinaryService>();
        _controller = new BrandController(_mockMediator.Object, _mockCloudinaryService.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "testadmin"),
            new Claim(ClaimTypes.NameIdentifier, "adminUserId"),
            new Claim(ClaimTypes.Role, "Admin"), // Phù hợp với [Authorize(Roles = "Manager, Admin")]
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    // === Tests for AddGift (Thêm Brand) ===
    [Fact]
    public async Task AddGift_ValidModelWithImage_ReturnsOkResult()
    {
        // Arrange
        var addBrandDto = new AddBrandDto //
        {
            Code = "BRAND001",
            Name = "Test Brand",
            ImageBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA"
        };
        var uploadedImageUrl = "http://cloudinary.com/image.png";
        var username = "testadmin"; // Từ mock ClaimsPrincipal

        _mockCloudinaryService.Setup(s => s.UploadImageBase64Async(addBrandDto.ImageBase64))
            .ReturnsAsync(uploadedImageUrl);

        _mockMediator.Setup(m => m.Send(It.Is<AddBrandCommand>(cmd =>
                cmd.Code == addBrandDto.Code &&
                cmd.Name == addBrandDto.Name &&
                cmd.Image == uploadedImageUrl &&
                cmd.CreatedBy == username
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.AddGift(addBrandDto); // Gọi đúng phương thức AddGift

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("Brand added successfully!", returnValue.Message); //

        _mockCloudinaryService.Verify(s => s.UploadImageBase64Async(addBrandDto.ImageBase64), Times.Once);
        _mockMediator.Verify(m => m.Send(It.IsAny<AddBrandCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddGift_ValidModelWithoutImage_ReturnsOkResult()
    {
        // Arrange
        var addBrandDto = new AddBrandDto //
        {
            Code = "BRAND002",
            Name = "Test Brand No Image",
            ImageBase64 = null
        };
         var username = "testadmin";

        _mockMediator.Setup(m => m.Send(It.Is<AddBrandCommand>(cmd =>
                cmd.Code == addBrandDto.Code &&
                cmd.Name == addBrandDto.Name &&
                cmd.Image == "" && // Controller sẽ gán "" nếu imageUrl là null
                cmd.CreatedBy == username
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.AddGift(addBrandDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("Brand added successfully!", returnValue.Message); //

        _mockCloudinaryService.Verify(s => s.UploadImageBase64Async(It.IsAny<string>()), Times.Never);
        _mockMediator.Verify(m => m.Send(It.IsAny<AddBrandCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task AddGift_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var addBrandDto = new AddBrandDto { Name = "Only Name" };
        _controller.ModelState.AddModelError("Code", "The Code field is required.");

        // Act
        var result = await _controller.AddGift(addBrandDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Contains("Code: The Code field is required.", returnValue.Message);
        Assert.Equal("VALIDATION_ERROR", returnValue.ErrorCode); //
    }

    [Fact]
    public async Task AddGift_MediatorThrowsInvalidOperation_ReturnsBadRequestWithBrandAddError()
    {
        // Arrange
        var addBrandDto = new AddBrandDto //
        {
            Code = "BRAND003",
            Name = "Existing Brand Code"
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<AddBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Brand code already exists."));

        // Act
        var result = await _controller.AddGift(addBrandDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Brand code already exists.", returnValue.Message);
        Assert.Equal("BRAND_ADD_ERROR", returnValue.ErrorCode); //
    }

    [Fact]
    public async Task AddGift_CloudinaryThrowsException_ReturnsServerError()
    {
        // Arrange
        var addBrandDto = new AddBrandDto //
        {
            Code = "BRAND004",
            Name = "Brand Cloudinary Fail",
            ImageBase64 = "dummy-base64-string"
        };

        _mockCloudinaryService.Setup(s => s.UploadImageBase64Async(addBrandDto.ImageBase64))
            .ThrowsAsync(new Exception("Cloudinary upload failed."));

        // Act
        var result = await _controller.AddGift(addBrandDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        var returnValue = Assert.IsType<ApiResponse<object>>(objectResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Cloudinary upload failed.", returnValue.Message);
        Assert.Equal("SERVER_ERROR", returnValue.ErrorCode); //
    }

    // === Tests for GetAllBrandForSelect ===
    [Fact]
    public async Task GetAllBrandForSelect_ReturnsOk_WithBrandList()
    {
        // Arrange
        var expectedBrands = new List<BrandNameCodeDto> // Giả sử BrandNameCodeDto được định nghĩa ở đâu đó
        {
            new BrandNameCodeDto("B001", "Brand 1", "img1.png"),
            new BrandNameCodeDto("B002", "Brand 2", "img2.png")
        };
        _mockMediator.Setup(m => m.Send(It.IsAny<GetAllBrandNamesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBrands);

        // Act
        var result = await _controller.GetAllBrandForSelect();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value); // Controller trả về ApiResponse<object>
        Assert.True(apiResponse.Success);
        Assert.Equal("Brand list retrieved successfully!", apiResponse.Message);
        var actualBrands = Assert.IsType<List<BrandNameCodeDto>>(apiResponse.Data);
        Assert.Equal(expectedBrands.Count, actualBrands.Count);
    }

    [Fact]
    public async Task GetAllBrandForSelect_MediatorThrowsException_ReturnsBadRequest()
    {
        // Arrange
        _mockMediator.Setup(m => m.Send(It.IsAny<GetAllBrandNamesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllBrandForSelect();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Equal("Database error", apiResponse.Message);
        Assert.Equal("BRAND_GET_ERROR", apiResponse.ErrorCode); //
    }


    // === Tests for GetAllBrands ===
    [Fact]
    public async Task GetAllBrands_ReturnsOk_WithPagedBrandList()
    {
        // Arrange
        var pageIndex = 0;
        var pageSize = 10;
        // Giả sử Brand là model của bạn và PagedModel<Brand> là kiểu trả về từ handler
        var brands = new List<Brand> { new Brand { Id = "1", Code = "B001", Name = "Brand 1"} };
        var pagedResult = new PagedModel<Brand>(brands.Count, brands, pageIndex, pageSize);

        _mockMediator.Setup(m => m.Send(It.Is<GetAllBrandQuery>(q => q.PageIndex == pageIndex && q.PageSize == pageSize), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAllBrands(pageIndex, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Brand list retrieved successfully!", apiResponse.Message);
        var actualPagedResult = Assert.IsType<PagedModel<Brand>>(apiResponse.Data);
        Assert.Equal(pagedResult.TotalCount, actualPagedResult.TotalCount);
    }

    // === Tests for DeleteBrand ===
    [Fact]
    public async Task DeleteBrand_ValidId_ReturnsOk()
    {
        // Arrange
        var brandIdsToDelete = new List<string> { "B001" };
        _mockMediator.Setup(m => m.Send(It.Is<DeleteBrandCommand>(cmd => cmd.Id.SequenceEqual(brandIdsToDelete)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Giả sử DeleteBrandCommand trả về bool

        // Act
        var result = await _controller.DeleteBrand(brandIdsToDelete);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Null(apiResponse.Data);
        Assert.Equal("Brand deleted successfully!", apiResponse.Message); //
    }

    [Fact]
    public async Task DeleteBrand_MediatorThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var brandIdsToDelete = new List<string> { "B001" };
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot delete brand, it is in use."));

        // Act
        var result = await _controller.DeleteBrand(brandIdsToDelete);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Equal("Cannot delete brand, it is in use.", apiResponse.Message);
        Assert.Equal("BRAND_DELETE_ERROR", apiResponse.ErrorCode); //
    }


    // === Tests for UpdateGift (Update Brand) ===
    [Fact]
    public async Task UpdateGift_ValidModel_ReturnsOk()
    {
        // Arrange
        var updateBrandDto = new UpdateBrandDto //
        {
            Id = "brandId1",
            Name = "Updated Name",
            ImageBase64 = "new-image-base64"
        };
        var uploadedImageUrl = "http://cloudinary.com/updated_image.png";
        var username = "testadmin";

        _mockCloudinaryService.Setup(s => s.UploadImageBase64Async(updateBrandDto.ImageBase64))
            .ReturnsAsync(uploadedImageUrl);
        _mockMediator.Setup(m => m.Send(It.Is<UpdateBrandCommand>(cmd =>
                cmd.Id == updateBrandDto.Id &&
                cmd.Name == updateBrandDto.Name &&
                cmd.Image == uploadedImageUrl && // Kiểm tra ảnh đã được upload
                cmd.UpdatedBy == username
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.UpdateGift(updateBrandDto); // Gọi đúng phương thức UpdateGift

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Brand updated successfully!", apiResponse.Message); //
    }

    [Fact]
    public async Task UpdateGift_ImageBase64IsHttpUrl_DoesNotCallCloudinary()
    {
        // Arrange
        var updateBrandDto = new UpdateBrandDto //
        {
            Id = "brandId1",
            Name = "Updated Name",
            ImageBase64 = "http://existing.com/image.png" // Đây là URL, không phải base64
        };
         var username = "testadmin";

        _mockMediator.Setup(m => m.Send(It.Is<UpdateBrandCommand>(cmd =>
                cmd.Id == updateBrandDto.Id &&
                cmd.Name == updateBrandDto.Name &&
                cmd.Image == null && // imageUrl sẽ là null vì ImageBase64 là http và controller gán imageUrl = null
                cmd.UpdatedBy == username
            ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.UpdateGift(updateBrandDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<object>>(okResult.Value);
        _mockCloudinaryService.Verify(s => s.UploadImageBase64Async(It.IsAny<string>()), Times.Never); // Không gọi Cloudinary
        _mockMediator.Verify(m => m.Send(It.IsAny<UpdateBrandCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task UpdateGift_MediatorThrowsInvalidOperation_ReturnsBadRequestWithGiftUpdateError()
    {
        // Arrange
        var updateBrandDto = new UpdateBrandDto { Id = "nonExistentId", Name = "Update Fail" }; //
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Brand not found."));

        // Act
        var result = await _controller.UpdateGift(updateBrandDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Equal("Brand not found.", apiResponse.Message);
        // Lưu ý: Controller gốc đang trả về "GIFT_UPDATE_ERROR" cho lỗi này.
        // Nếu đây là lỗi copy-paste, bạn có thể muốn sửa nó trong controller thành "BRAND_UPDATE_ERROR".
        // Unit test sẽ kiểm tra theo code hiện tại.
        Assert.Equal("GIFT_UPDATE_ERROR", apiResponse.ErrorCode); //
    }

    // === Tests for GetBrandsByCategory ===
    [Fact]
    public async Task GetBrandsByCategory_ValidCategoryCode_ReturnsOkWithBrands()
    {
        // Arrange
        var categoryCode = "CAT001";
        var expectedBrands = new List<BrandNameCodeDto> { new BrandNameCodeDto("B001", "Brand For Cat001", null) };
        _mockMediator.Setup(m => m.Send(It.Is<GetBrandsByCategoryQuery>(q => q.CategoryCode == categoryCode), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBrands);

        // Act
        var result = await _controller.GetBrandsByCategory(categoryCode);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<List<BrandNameCodeDto>>>(okResult.Value); // Controller trả về ApiResponse<List<BrandNameCodeDto>>
        Assert.True(apiResponse.Success);
        Assert.Equal($"Retrieved brands for category '{categoryCode}' successfully!", apiResponse.Message);
        Assert.Equal(expectedBrands.Count, apiResponse.Data.Count);
    }

    [Fact]
    public async Task GetBrandsByCategory_EmptyCategoryCode_ReturnsBadRequest()
    {
        // Arrange
        var categoryCode = " "; // Hoặc null, hoặc string.Empty

        // Act
        var result = await _controller.GetBrandsByCategory(categoryCode);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Equal("Category code is required", apiResponse.Message);
        Assert.Equal("VALIDATION_ERROR", apiResponse.ErrorCode); //
    }

     [Fact]
    public async Task GetBrandsByCategory_MediatorThrowsException_ReturnsServerError()
    {
        // Arrange
        var categoryCode = "CAT001";
        _mockMediator.Setup(m => m.Send(It.IsAny<GetBrandsByCategoryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Some internal error."));

        // Act
        var result = await _controller.GetBrandsByCategory(categoryCode);

        // Assert
        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, serverErrorResult.StatusCode);
        var apiResponse = Assert.IsType<ApiResponse<object>>(serverErrorResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Equal("Some internal error.", apiResponse.Message);
        Assert.Equal("SERVER_ERROR", apiResponse.ErrorCode); //
    }
}

// Giả định các DTO và Model cần thiết (nếu chưa có, bạn cần tạo chúng hoặc điều chỉnh namespace)
// public class AddBrandDto // Có trong Queries/Brand/BrandDto.cs
// {
//     public string Code { get; set; }
//     public string Name { get; set; }
//     public string ImageBase64 { get; set; }
// }

// public class UpdateBrandDto // Có trong Queries/Brand/BrandDto.cs
// {
//     public string Id { get; set; }
//     public string Name { get; set; }
//     public string ImageBase64 { get; set; }
// }

// public class BrandNameCodeDto // Có trong Handlers/Brand/GetAllBrandHandler.cs
// {
//     public string Code { get; set; }
//     public string Name { get; set; }
//     public string ImageUrl { get; set; }
//     public BrandNameCodeDto(string code, string name, string imageUrl)
//     {
//         Code = code;
//         Name = name;
//         ImageUrl = imageUrl;
//     }
// }

// public class Brand // Model cơ bản, giả sử có trong Models/Brand/Brand.cs
// {
//     public string Id { get; set; }
//     public string Code { get; set; }
//     public string Name { get; set; }
//     public string Image { get; set; }
// }

// public class PagedModel<T> // Có trong Models/PageModel.cs (OMS.Core.Queries)
// {
//     public long TotalCount { get; }
//     public IEnumerable<T> Data { get; }
//     public int PageIndex { get; }
//     public int PageSize { get; }
//     public PagedModel(long totalCount, IEnumerable<T> data, int pageIndex, int pageSize)
//     {
//         TotalCount = totalCount;
//         Data = data;
//         PageIndex = pageIndex;
//         PageSize = pageSize;
//     }
// }

// --- Các Command và Query giả định (nếu chưa có, bạn cần tạo chúng hoặc điều chỉnh namespace) ---
// public class GetAllBrandNamesQuery : IRequest<List<BrandNameCodeDto>> { }
// public class GetAllBrandQuery : IRequest<PagedModel<Brand>>
// {
//     public int PageIndex { get; }
//     public int PageSize { get; }
//     public GetAllBrandQuery(int pageIndex, int pageSize) { PageIndex = pageIndex; PageSize = pageSize; }
// }
// public class AddBrandCommand : IRequest<Unit>
// {
//     public string Code { get; }
//     public string Name { get; }
//     public string Image { get; }
//     public string CreatedBy { get; }
//     public AddBrandCommand(string code, string name, string image, string createdBy)
//     { Code = code; Name = name; Image = image; CreatedBy = createdBy;}
// }
// public class DeleteBrandCommand : IRequest<bool> // Controller không check bool, chỉ Send
// {
//     public IEnumerable<string> Id { get; }
//     public DeleteBrandCommand(IEnumerable<string> id) { Id = id; }
// }
// public class UpdateBrandCommand : IRequest<Unit>
// {
//     public string Id { get; }
//     public string Name { get; }
//     public string Image { get; }
//     public string UpdatedBy { get; }
//     public UpdateBrandCommand(string id, string name, string image, string updatedBy)
//     { Id = id; Name = name; Image = image; UpdatedBy = updatedBy; }
// }
// public class GetBrandsByCategoryQuery : IRequest<List<BrandNameCodeDto>>
// {
//     public string CategoryCode { get; }
//     public GetBrandsByCategoryQuery(string categoryCode) { CategoryCode = categoryCode; }
// }

// public class ApiResponse<T> // Có trong Queries/Response.cs
// {
//     public bool Success { get; set; }
//     public string Message { get; set; }
//     public HttpStatusCode StatusCode { get; set; }
//     public T Data { get; set; }
//     public string ErrorCode { get; set; }
//     public DateTime Timestamp { get; set; }

//     public static ApiResponse<T> CreateSuccess(T data, string message) => new ApiResponse<T> { Success = true, Data = data, Message = message, StatusCode = HttpStatusCode.OK, Timestamp = DateTime.UtcNow };
//     public static ApiResponse<T> CreateError(string message, HttpStatusCode statusCode, string errorCode) => new ApiResponse<T> { Success = false, Message = message, StatusCode = statusCode, ErrorCode = errorCode, Timestamp = DateTime.UtcNow };
// }