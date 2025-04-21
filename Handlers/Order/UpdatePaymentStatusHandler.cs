// Handlers/Order/UpdatePaymentStatusHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class UpdatePaymentStatusHandler : IRequestHandler<UpdatePaymentStatusCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string ORDERS_KEY = "orders";

    public UpdatePaymentStatusHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<bool> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
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
        
        // Validate payment status transition
        ValidatePaymentStatusTransition(order.PaymentStatus, request.NewStatus);
        
        // Update payment status
        order.UpdatePaymentStatus(request.NewStatus, request.UpdatedBy);
        
        // Save changes
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            ORDERS_KEY,
            orders,
            cancellationToken: cancellationToken
        );
        
        return true;
    }
    
    private void ValidatePaymentStatusTransition(PaymentStatus currentStatus, PaymentStatus newStatus)
    {
        bool isValid = false;
        
        switch (currentStatus)
        {
            case PaymentStatus.Pending:
                isValid = newStatus == PaymentStatus.Paid || newStatus == PaymentStatus.Failed;
                break;
            case PaymentStatus.Failed:
                isValid = newStatus == PaymentStatus.Pending || newStatus == PaymentStatus.Paid;
                break;
            case PaymentStatus.Paid:
                isValid = newStatus == PaymentStatus.Refunded;
                break;
            case PaymentStatus.Refunded:
                isValid = false; // Terminal state
                break;
        }
        
        if (!isValid)
        {
            throw new InvalidOperationException($"Không thể chuyển trạng thái thanh toán từ {currentStatus} sang {newStatus}");
        }
    }
}