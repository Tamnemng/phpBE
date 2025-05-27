using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Dapr.Client;
using Think4.Services; // Namespace của AuthService và User
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System;

public class AuthServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockDaprClient = new Mock<DaprClient>();

        // Mock IConfiguration cho Jwt:Secret nếu cần thiết cho việc generate token (mặc dù Authenticate không dùng trực tiếp)
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(s => s.Value).Returns("YourVeryLongSecretKeyHereForThink4ApiSecurity123!");
        _mockConfiguration.Setup(c => c.GetSection("Jwt:Secret")).Returns(mockJwtSection.Object);
        // Hoặc bạn có thể mock trực tiếp key: _mockConfiguration.Setup(c => c["Jwt:Secret"]).Returns("YourVeryLongSecretKeyHereForThink4ApiSecurity123!");


        _authService = new AuthService(_mockConfiguration.Object, _mockDaprClient.Object);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    [Fact]
    public async Task Authenticate_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var username = "admin";
        var password = "123123123";
        var hashedPassword = HashPassword(password);
        var fakeUser = new User(username, hashedPassword, "test@example.com", "Test User", UserRole.User);

        var usersList = new List<User> { fakeUser };

        _mockDaprClient.Setup(client => client.GetStateAsync<List<User>>(
                             "statestore", // STORE_NAME
                             "users",      // USERS_KEY
                             It.IsAny<ConsistencyMode?>(),
                             It.IsAny<IReadOnlyDictionary<string, string>?>(),
                             It.IsAny<CancellationToken>()))
                         .ReturnsAsync(usersList);

        _mockDaprClient.Setup(client => client.SaveStateAsync(
                            "statestore",
                            "users",
                            It.IsAny<List<User>>(),
                            It.IsAny<StateOptions?>(),
                            It.IsAny<IReadOnlyDictionary<string, string>?>(),
                            It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);


        // Act
        var result = await _authService.Authenticate(username, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.True(result.LastActive.HasValue); // Kiểm tra LastActive đã được cập nhật
    }

    [Fact]
    public async Task Authenticate_InvalidUsername_ReturnsNull()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "password123";
        var usersList = new List<User>(); // Không có user nào trong state

        _mockDaprClient.Setup(client => client.GetStateAsync<List<User>>(
                             "statestore", "users", null, null, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(usersList);

        // Act
        var result = await _authService.Authenticate(username, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var username = "testuser";
        var correctPassword = "password123";
        var incorrectPassword = "wrongpassword";
        var hashedPassword = HashPassword(correctPassword);
        var fakeUser = new User(username, hashedPassword, "test@example.com", "Test User", UserRole.User);
        var usersList = new List<User> { fakeUser };

        _mockDaprClient.Setup(client => client.GetStateAsync<List<User>>(
                             "statestore", "users", null, null, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(usersList);

        // Act
        var result = await _authService.Authenticate(username, incorrectPassword);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_EmptyUsername_ReturnsNull()
    {
        // Act
        var result = await _authService.Authenticate("", "password123");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_EmptyPassword_ReturnsNull()
    {
        // Act
        var result = await _authService.Authenticate("testuser", "");

        // Assert
        Assert.Null(result);
    }
}