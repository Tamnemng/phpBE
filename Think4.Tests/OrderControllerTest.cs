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
using OMS.Core.Queries; // Cho PagedModel
// using Think4.Models; // Cho Order, OrderStatus, CartItemType, PaymentMethod, etc.
// using Think4.Queries.Order; // Cho CreateOrderDto, OrderDetailDto, UpdateOrderStatusDto, etc.
// using Think4.Queries; // Cho ApiResponse

public class OrderControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new OrderController(_mockMediator.Object); //

        SetupControllerContext("defaultUserId", "defaultUser", "User");
    }

    private void SetupControllerContext(string userId, string username, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task CreateOrder_ValidModel_ReturnsOkWithOrderDetailDto()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto //
        {
            SelectedItems = new List<OrderItemSelectionDto> { new OrderItemSelectionDto { ItemId = "item1", ItemType = CartItemType.Product, Quantity = 1 } }, //
            PaymentMethod = PaymentMethod.COD, //
            CustomerName = "Test Customer",
            CustomerPhone = "123456789",
            ShippingAddress = "123 Test St",
            CustomerEmail = "test@example.com"
        };
        // Cần một đối tượng Order để khởi tạo OrderDetailDto
        var fakeOrder = new Order() { Id = "order123", UserId = "defaultUserId" /* Thêm các thuộc tính khác nếu cần */}; //
        var expectedOrderDetailDto = new OrderDetailDto(fakeOrder); //


        _mockMediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>())) //
            .ReturnsAsync(expectedOrderDetailDto);

        // Act
        var result = await _controller.CreateOrder(createOrderDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<OrderDetailDto>>(okResult.Value); //
        Assert.True(apiResponse.Success);
        Assert.Equal(expectedOrderDetailDto.Id, apiResponse.Data.Id);
        Assert.Equal("Order placed successfully!", apiResponse.Message); //
    }

    [Fact]
    public async Task CreateOrder_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto(); // Thiếu các trường bắt buộc
        _controller.ModelState.AddModelError("CustomerName", "The CustomerName field is required.");

        // Act
        var result = await _controller.CreateOrder(createOrderDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(badRequestResult.Value); //
        Assert.False(apiResponse.Success);
        Assert.Contains("CustomerName: The CustomerName field is required.", apiResponse.Message);
        Assert.Equal("VALIDATION_ERROR", apiResponse.ErrorCode); //
    }

    [Fact]
    public async Task GetUserOrders_ReturnsOkWithPagedOrders()
    {
        // Arrange
        var userId = "defaultUserId"; // Lấy từ SetupControllerContext
        var pageIndex = 0;
        var pageSize = 10;
        var orderSummaries = new List<OrderSummaryDto>(); //
        var pagedResult = new PagedModel<OrderSummaryDto>(0, orderSummaries, pageIndex, pageSize); //

        _mockMediator.Setup(m => m.Send(It.Is<GetUserOrdersQuery>(q => q.UserId == userId && q.PageIndex == pageIndex && q.PageSize == pageSize), It.IsAny<CancellationToken>())) //
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetUserOrders(pageIndex, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value); //
        Assert.True(apiResponse.Success);
        Assert.Same(pagedResult, apiResponse.Data); // Controller trả về object PagedModel trực tiếp
        Assert.Equal("User order list retrieved successfully!", apiResponse.Message); //
    }

    [Fact]
    public async Task GetOrderById_UserIsOwner_ReturnsOk()
    {
        // Arrange
        var orderId = "order1";
        var userId = "defaultUserId";
        var fakeOrder = new Order { Id = orderId, UserId = userId }; //
        var orderDetailDto = new OrderDetailDto(fakeOrder); //

        _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId && q.UserId == userId), It.IsAny<CancellationToken>())) //
            .ReturnsAsync(orderDetailDto);

        // Act
        var result = await _controller.GetOrderById(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<OrderDetailDto>>(okResult.Value); //
        Assert.True(apiResponse.Success);
        Assert.Equal("Order details retrieved successfully!", apiResponse.Message); //
        Assert.Equal(orderId, apiResponse.Data.Id);
    }

    [Fact]
    public async Task GetOrderById_UserIsAdmin_ReturnsOk()
    {
        // Arrange
        SetupControllerContext("adminId", "adminUser", "Admin"); // Giả lập admin
        var orderId = "order1";
        var fakeOrder = new Order { Id = orderId, UserId = "anotherUserId" }; //
        var orderDetailDto = new OrderDetailDto(fakeOrder); //

        // Admin sẽ truyền null cho userId trong GetOrderByIdQuery
        _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId && q.UserId == null), It.IsAny<CancellationToken>())) //
            .ReturnsAsync(orderDetailDto);

        // Act
        var result = await _controller.GetOrderById(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<OrderDetailDto>>(okResult.Value); //
        Assert.True(apiResponse.Success);
    }


    [Fact]
    public async Task GetOrderById_ThrowsUnauthorizedAccess_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = "order1";
        // User hiện tại không phải admin, và GetOrderByIdQuery sẽ ném UnauthorizedAccessException
        _mockMediator.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>())) //
            .ThrowsAsync(new UnauthorizedAccessException("Access denied."));

        // Act
        var result = await _controller.GetOrderById(orderId);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(unauthorizedResult.Value); //
        Assert.False(apiResponse.Success);
        Assert.Equal("Access denied.", apiResponse.Message);
        Assert.Equal("UNAUTHORIZED", apiResponse.ErrorCode); //
    }


    [Fact]
    public async Task UpdateOrderStatus_ValidDtoAsAdmin_ReturnsOk()
    {
        // Arrange
        SetupControllerContext("adminId", "adminUser", "Admin"); // Yêu cầu role Admin/Manager
        var updateDto = new UpdateOrderStatusDto { OrderId = "order1", NewStatus = OrderStatus.Processing }; //
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true); //

        // Act
        var result = await _controller.UpdateOrderStatus(updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value); //
        Assert.True(apiResponse.Success);
        Assert.Equal("Order status updated successfully!", apiResponse.Message); //
    }


    [Fact]
    public async Task CancelOrder_UserIsOwner_ReturnsOk()
    {
        // Arrange
        var orderId = "orderToCancel";
        var userId = "defaultUserId"; // User từ SetupControllerContext
        var username = "defaultUser";
        SetupControllerContext(userId, username, "User");

        // Giả lập GetOrderByIdQuery thành công
        var fakeOrder = new Order { Id = orderId, UserId = userId }; //
        _mockMediator.Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId && q.UserId == userId), It.IsAny<CancellationToken>())) //
            .ReturnsAsync(new OrderDetailDto(fakeOrder)); //

        // Giả lập UpdateOrderStatusCommand thành công
        _mockMediator.Setup(m => m.Send(It.Is<UpdateOrderStatusCommand>(cmd => cmd.OrderId == orderId && cmd.NewStatus == OrderStatus.Canceled && cmd.UpdatedBy == username), It.IsAny<CancellationToken>())) //
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CancelOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value); //
        Assert.True(apiResponse.Success);
        Assert.Equal("Order canceled successfully!", apiResponse.Message); //
    }

    // Viết thêm tests cho GetAllOrders, UpdatePaymentStatus.
}