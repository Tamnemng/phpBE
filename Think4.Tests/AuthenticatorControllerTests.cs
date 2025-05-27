using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Think4.Services; // Namespace của IAuthService và các DTOs liên quan đến User
// using Think4.Queries; // Nếu ApiResponse ở namespace riêng
// using Think4.Models; // Nếu User, UserRole ở namespace riêng

namespace Think4.Tests
{
    public class AuthenticatorControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthenticatorControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // Sẽ thiết lập User khi cần
            };
        }

        private void SetupUserClaims(string userId, string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = claimsPrincipal;
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithLoginResponse()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "testuser", Password = "password" };
            var user = new User(loginDto.Username, "hashedPassword", "test@example.com", "Test User", UserRole.User);
            var token = "sample.jwt.token";
            _mockAuthService.Setup(s => s.Authenticate(loginDto.Username, loginDto.Password)).ReturnsAsync(user);
            _mockAuthService.Setup(s => s.GenerateJwtToken(user)).Returns(token);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(token, apiResponse.Data.Token);
            Assert.Equal(user.Username, apiResponse.Data.User.Username);
            Assert.Equal("Login successful!", apiResponse.Message);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new UserLoginDto { Username = "testuser", Password = "wrong" };
            _mockAuthService.Setup(s => s.Authenticate(loginDto.Username, loginDto.Password)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Invalid username or password", apiResponse.Message);
            Assert.Equal("AUTH_ERROR", apiResponse.ErrorCode);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsOk()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Username = "newuser", Password = "password123", Email = "new@example.com", FullName = "New User" };
            var createdUser = new User(registerDto.Username, "hashed", registerDto.Email, registerDto.FullName, UserRole.User);
            _mockAuthService.Setup(s => s.Register(registerDto)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(createdUser.Username, apiResponse.Data.Username);
            Assert.Equal("Registration successful!", apiResponse.Message);
        }

        [Fact]
        public async Task Register_UsernameTaken_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new UserRegisterDto { Username = "existinguser", Password = "password123", Email = "new@example.com", FullName = "New User" };
            _mockAuthService.Setup(s => s.Register(registerDto)).ThrowsAsync(new InvalidOperationException("Username 'existinguser' is already taken"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Username 'existinguser' is already taken", apiResponse.Message);
            Assert.Equal("REGISTER_ERROR", apiResponse.ErrorCode);
        }

        [Fact]
        public async Task RegisterManager_ValidModel_ReturnsOk()
        {
            // Arrange
            SetupUserClaims("adminId", "adminUser", "Admin"); // Action này yêu cầu role Admin
            var managerDto = new ManagerRegisterDto { Username = "manager", Password = "password", Email = "manager@example.com", FullName = "Manager User" };
            var createdManager = new User(managerDto.Username, "hashed", managerDto.Email, managerDto.FullName, UserRole.Manager);
            _mockAuthService.Setup(s => s.RegisterManager(managerDto)).ReturnsAsync(createdManager);

            // Act
            var result = await _controller.RegisterManager(managerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("Manager account registration successful!", apiResponse.Message);
            Assert.Equal(UserRole.Manager, apiResponse.Data.Role);
        }

        [Fact]
        public async Task CheckSession_ActiveSession_ReturnsOkWithUserDto()
        {
            // Arrange
            var userId = "testUserId";
            SetupUserClaims(userId, "testuser", "User");
            var user = new User("testuser", "hashed", "test@example.com", "Test User", UserRole.User) { Id = userId };

            _mockAuthService.Setup(s => s.IsSessionActive(userId)).ReturnsAsync(true);
            _mockAuthService.Setup(s => s.UpdateUserLastActive(userId)).Returns(Task.CompletedTask);
            _mockAuthService.Setup(s => s.GetUserById(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.CheckSession();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(user.Username, apiResponse.Data.Username);
            Assert.Equal("Session active", apiResponse.Message);
        }

        [Fact]
        public async Task CheckSession_InactiveSession_ReturnsUnauthorized()
        {
            // Arrange
            var userId = "testUserId";
            SetupUserClaims(userId, "testuser", "User");
            _mockAuthService.Setup(s => s.IsSessionActive(userId)).ReturnsAsync(false);

            // Act
            var result = await _controller.CheckSession();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Session expired", apiResponse.Message);
            Assert.Equal("SESSION_EXPIRED", apiResponse.ErrorCode);
        }

        [Fact]
        public async Task Logout_UserAuthenticated_ReturnsOk()
        {
            // Arrange
            var userId = "testUserId";
            SetupUserClaims(userId, "testuser", "User");
            _mockAuthService.Setup(s => s.LogoutUser(userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Null(apiResponse.Data);
            Assert.Equal("Logout successful!", apiResponse.Message);
        }

        // Các test cases cho GetCurrentUser và UpdateCurrentUser đã được cung cấp trong các phản hồi trước
        // và có thể được điều chỉnh tương tự nếu cần.
    }
}