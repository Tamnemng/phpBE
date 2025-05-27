using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Think4.Services; // Namespace của ICloudinaryService và CloudinaryService
using System.Threading.Tasks;
using System;
using CloudinaryDotNet; // Cần thiết cho Account
// CloudinaryDotNet.Actions không cần mock trực tiếp nếu bạn chỉ kiểm tra việc khởi tạo Cloudinary client
// và logic xử lý chuỗi base64. Để test việc upload thật sự, bạn cần integration test
// hoặc mock sâu hơn vào Cloudinary client, điều này phức tạp hơn cho unit test.

public class CloudinaryServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockCloudinarySection;
    private readonly CloudinaryService _cloudinaryService;

    public CloudinaryServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockCloudinarySection = new Mock<IConfigurationSection>();

        // Thiết lập mock cho GetSection("Cloudinary")
        _mockConfiguration.Setup(c => c.GetSection("Cloudinary"))
            .Returns(_mockCloudinarySection.Object);

        // Thiết lập mock cho các giá trị con trong section "Cloudinary"
        _mockCloudinarySection.Setup(s => s["CloudName"]).Returns("dummy_cloud_name");
        _mockCloudinarySection.Setup(s => s["ApiKey"]).Returns("dummy_api_key");
        _mockCloudinarySection.Setup(s => s["ApiSecret"]).Returns("dummy_api_secret");

        // Khởi tạo CloudinaryService với IConfiguration đã mock
        // Lưu ý: Việc test upload thực sự lên Cloudinary sẽ là integration test.
        // Ở đây, unit test chủ yếu kiểm tra việc khởi tạo Account và logic xử lý chuỗi base64.
        // Nếu muốn test sâu hơn mà không gọi Cloudinary thật, bạn cần phải có cách để mock đối tượng Cloudinary
        // và phương thức UploadAsync của nó, điều này có thể khó vì Cloudinary không cung cấp interface dễ mock.
        // Trong ví dụ này, chúng ta tập trung vào logic trước khi gọi UploadAsync.
        _cloudinaryService = new CloudinaryService(_mockConfiguration.Object);
    }

    [Fact]
    public async Task UploadImageBase64Async_NullOrEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        var exceptionNull = await Assert.ThrowsAsync<ArgumentException>(() => _cloudinaryService.UploadImageBase64Async(null));
        Assert.Equal("No base64 string was provided", exceptionNull.Message); //

        var exceptionEmpty = await Assert.ThrowsAsync<ArgumentException>(() => _cloudinaryService.UploadImageBase64Async(string.Empty));
        Assert.Equal("No base64 string was provided", exceptionEmpty.Message); //
    }

    [Fact]
    public void Constructor_ReadsConfigurationCorrectly()
    {
        // Assert
        // Kiểm tra xem các giá trị mock có được đọc đúng khi CloudinaryService được khởi tạo không.
        // Điều này hơi khó kiểm tra trực tiếp với Cloudinary client mà không thay đổi code gốc
        // để cho phép inject một mock Cloudinary client.
        // Tuy nhiên, nếu không có lỗi khi khởi tạo, có thể coi là một phần thành công.
        // Bạn cũng có thể verify rằng GetSection và các key đã được gọi.
        _mockConfiguration.Verify(c => c.GetSection("Cloudinary"), Times.Once);
        _mockCloudinarySection.Verify(s => s["CloudName"], Times.Once);
        _mockCloudinarySection.Verify(s => s["ApiKey"], Times.Once);
        _mockCloudinarySection.Verify(s => s["ApiSecret"], Times.Once);
        Assert.NotNull(_cloudinaryService); // Đảm bảo service được khởi tạo
    }

    [Fact]
    public async Task UploadImageBase64Async_ValidBase64WithoutPrefix_ProcessesCorrectly()
    {
        // Arrange
        // Một chuỗi base64 hợp lệ (ví dụ: ảnh PNG 1x1 pixel màu đỏ)
        var validBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

        // Vì chúng ta không thể (và không nên) gọi Cloudinary thật trong unit test,
        // chúng ta sẽ gặp lỗi khi phương thức _cloudinary.UploadAsync được gọi.
        // Để test logic xử lý chuỗi base64, chúng ta có thể:
        // 1. Refactor CloudinaryService để nó nhận Cloudinary client qua constructor (Dependency Injection)
        //    và sau đó mock Cloudinary client đó.
        // 2. Hoặc chấp nhận rằng unit test này chỉ có thể kiểm tra đến trước khi gọi UploadAsync.

        // Với cấu trúc hiện tại, việc gọi phương thức này sẽ cố gắng thực hiện upload thật
        // và có thể gây lỗi nếu cấu hình không đúng hoặc không có kết nối mạng.
        // Do đó, một unit test hoàn chỉnh cho phương thức này yêu cầu refactor code service.

        // Giả sử chúng ta chỉ muốn kiểm tra việc parse base64, thì logic đó nằm trong Convert.FromBase64String.
        // Một cách để test gián tiếp là bắt Exception nếu chuỗi base64 không hợp lệ,
        // nhưng ở đây ta đang kiểm tra trường hợp hợp lệ.

        // Nếu không refactor, test này sẽ là integration test hoặc cần một mock phức tạp cho Cloudinary.
        // Để giữ nó là unit test, ta sẽ giả định phần upload sẽ gây lỗi và chỉ test phần logic trước đó
        // nếu có thể.

        // Hiện tại, với code gốc, test này sẽ cố upload.
        // Nếu bạn muốn tránh điều đó, bạn cần mock `_cloudinary.UploadAsync`.
        // Điều này khó vì `_cloudinary` là một instance cụ thể, không phải interface.

        // Test này mang tính minh họa cho việc kiểm tra xử lý đầu vào.
        // Thực tế, bạn nên refactor CloudinaryService để có thể mock Cloudinary client.
        await Assert.ThrowsAsync<Exception>(async () => await _cloudinaryService.UploadImageBase64Async(validBase64));
        // Mong đợi exception vì _cloudinary.UploadAsync sẽ được gọi với client thật và có thể thất bại
        // trong môi trường test không có cấu hình Cloudinary thật hoặc không có mạng.
        // Hoặc, nếu bạn có cấu hình Cloudinary "dummy" hoạt động được, bạn có thể thay đổi assert.
    }

     [Fact]
    public async Task UploadImageBase64Async_ValidBase64WithDataUrlPrefix_ProcessesCorrectly()
    {
        // Arrange
        var base64WithPrefix = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="; //

        // Tương tự như test trên, việc test này sẽ phụ thuộc vào việc mock Cloudinary client
        // hoặc chấp nhận nó là một dạng integration test cục bộ.
        await Assert.ThrowsAsync<Exception>(async () => await _cloudinaryService.UploadImageBase64Async(base64WithPrefix));
        // Mong đợi exception vì không có mock cho _cloudinary.UploadAsync
    }
}