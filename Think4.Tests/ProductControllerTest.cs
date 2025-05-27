using Xunit;
using Moq;
using Dapr.Client;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Think4.Services; // Cho ICloudinaryService
// using YourProject.Models; // Cho Product, BrandMetaData, CategoryMetaData, GiftMetaData, Image, Gift, ProductVariant, VariantGroup, Option
// using YourProject.Queries.Product.create; // Cho AddProductCommand
// using YourProject.Utilities; // Cho IdGenerator nếu bạn muốn kiểm soát Id được tạo

public class AddProductCommandHandlerTests
{
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly Mock<ICloudinaryService> _mockCloudinaryService;
    private readonly AddProductCommandHandler _handler;

    public AddProductCommandHandlerTests()
    {
        _mockDaprClient = new Mock<DaprClient>();
        _mockCloudinaryService = new Mock<ICloudinaryService>();
        _handler = new AddProductCommandHandler(_mockDaprClient.Object, _mockCloudinaryService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesProductVariantsAndReturnsUnit()
    {
        // Arrange
        var command = new AddProductCommand //
        {
            Name = "Test Product",
            Code = "PROD001",
            ImageUrl = "http://example.com/main.jpg", // Giả sử đã được xử lý bởi controller
            CategoriesCode = new List<string> { "CAT01" },
            BrandCode = "BRAND01",
            Status = ProductStatus.InStock,
            CreatedBy = "testuser",
            GiftCodes = new List<string> { "GIFT01" },
            Variants = new List<VariantGroup>
            {
                new VariantGroup
                {
                    OptionTitle = "Color",
                    Options = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            OptionLabel = "Red",
                            OriginalPrice = 100,
                            CurrentPrice = 90,
                            ImagesBase64 = new List<ImageBase64Dto> { new ImageBase64Dto("base64_red_img", 1) }, //
                            Descriptions = new List<Description>(),
                            ShortDescription = "Red variant"
                        }
                    }
                }
            }
        };

        var existingProducts = new List<Product>();
        var brands = new List<BrandMetaData> { new BrandMetaData { Code = "BRAND01", Name = "Test Brand" } }; //
        var categories = new List<CategoryMetaData> { new CategoryMetaData { Code = "CAT01", Name = "Test Category" } }; //
        var giftsMeta = new List<GiftMetaData> { new GiftMetaData { Code = "GIFT01", Name = "Test Gift", Image = "gift.jpg"} }; //
        var uploadedImageUrl = "http://cloudinary.com/red_image.png";

        _mockDaprClient.Setup(c => c.GetStateAsync<List<Product>>("statestore", "products", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProducts);
        _mockDaprClient.Setup(c => c.GetStateAsync<List<BrandMetaData>>("statestore", "brands", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(brands);
        _mockDaprClient.Setup(c => c.GetStateAsync<List<CategoryMetaData>>("statestore", "categories", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        _mockDaprClient.Setup(c => c.GetStateAsync<List<GiftMetaData>>("statestore", "gifts", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(giftsMeta);

        _mockCloudinaryService.Setup(s => s.UploadImageBase64Async(It.IsAny<string>()))
            .ReturnsAsync(uploadedImageUrl);

        _mockDaprClient.Setup(c => c.SaveStateAsync("statestore", "products", It.IsAny<List<Product>>(), null, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        _mockDaprClient.Verify(c => c.SaveStateAsync("statestore", "products",
            It.Is<List<Product>>(list =>
                list.Any(p => p.ProductInfo.Code == command.Code &&
                              p.ProductOptions.Any(po => po.Title == "Color" && po.Options.Any(o => o.Label == "Red" && o.Selected)) &&
                              p.ProductDetail.Image.Any(img => img.Url == uploadedImageUrl) && // Kiểm tra ảnh variant
                              p.Gifts.Any(g => g.Code == "GIFT01") // Kiểm tra gift
                )),
            null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockCloudinaryService.Verify(s => s.UploadImageBase64Async("base64_red_img"), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductCodeExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new AddProductCommand { Code = "EXISTING001" /* các thuộc tính khác */ }; //
        var existingProducts = new List<Product>
        {
            new Product { ProductInfo = new ProductInfo { Code = "EXISTING001" } } //
        };
        _mockDaprClient.Setup(c => c.GetStateAsync<List<Product>>("statestore", "products", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProducts);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Product with code 'EXISTING001' already exists.", exception.Message); //
    }

    [Fact]
    public async Task Handle_BrandNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new AddProductCommand { BrandCode = "UNKNOWN_BRAND", CategoriesCode = new List<string>(), Variants = new List<VariantGroup>() }; //
        _mockDaprClient.Setup(c => c.GetStateAsync<List<Product>>("statestore", "products", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        _mockDaprClient.Setup(c => c.GetStateAsync<List<BrandMetaData>>("statestore", "brands", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BrandMetaData>()); // Không có brand nào

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Brand with code 'UNKNOWN_BRAND' does not exist.", exception.Message); //
    }

    // Thêm các test cases khác cho CategoryNotFound, GiftNotFound, VariantValidation, CloudinaryUploadFailed...
}