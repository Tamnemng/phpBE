// Handlers/Order/UpdateOrderStatusHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string ORDERS_KEY = "orders";

    public UpdateOrderStatusHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
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
        
        // Validate status transition
        ValidateStatusTransition(order.Status, request.NewStatus);
        
        // Update order status
        order.UpdateStatus(request.NewStatus, request.UpdatedBy);
        
        // Save changes
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            ORDERS_KEY,
            orders,
            cancellationToken: cancellationToken
        );
        
        return true;
    }
    
    private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        bool isValid = false;
        
        switch (currentStatus)
        {
            case OrderStatus.Pending:
                isValid = newStatus == OrderStatus.Confirmed || newStatus == OrderStatus.Canceled;
                break;
            case OrderStatus.Confirmed:
                isValid = newStatus == OrderStatus.Processing || newStatus == OrderStatus.Canceled;
                break;
            case OrderStatus.Processing:
                isValid = newStatus == OrderStatus.Shipping || newStatus == OrderStatus.Canceled;
                break;
            case OrderStatus.Shipping:
                isValid = newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Canceled;
                break;
            case OrderStatus.Delivered:
                isValid = newStatus == OrderStatus.Completed;
                break;
            // Canceled and Completed are terminal states
            case OrderStatus.Canceled:
            case OrderStatus.Completed:
                isValid = false;
                break;
        }
        
        if (!isValid)
        {
            throw new InvalidOperationException($"Không thể chuyển trạng thái từ {currentStatus} sang {newStatus}");
        }
    }
}