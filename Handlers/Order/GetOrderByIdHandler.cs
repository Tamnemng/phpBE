// Handlers/Order/GetOrderByIdHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string ORDERS_KEY = "orders";

    public GetOrderByIdHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<OrderDetailDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orders = await _daprClient.GetStateAsync<List<Order>>(
            STORE_NAME,
            ORDERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Order>();
        
        var order = orders.FirstOrDefault(o => o.Id == request.OrderId);
        
        if (order == null)
        {
            throw new InvalidOperationException($"Không tìm thấy đơn hàng với ID: {request.OrderId}");
        }
        
        // If user ID provided, check if the order belongs to this user (for security)
        if (!string.IsNullOrEmpty(request.UserId) && request.UserId != order.UserId)
        {
            throw new UnauthorizedAccessException("Không có quyền truy cập đơn hàng này.");
        }
        
        return new OrderDetailDto(order);
    }
}