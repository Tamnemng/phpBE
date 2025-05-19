using MediatR;
using System.Collections.Generic;
using OMS.Core.Queries;

// Create Order Command
public class CreateOrderCommand : IRequest<OrderDetailDto>
{
    public string UserId { get; set; }
    public List<OrderItemSelectionDto> SelectedItems { get; set; } // Thêm thuộc tính này
    public PaymentMethod PaymentMethod { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string ShippingAddress { get; set; }
    public string CustomerEmail { get; set; }
    public string Notes { get; set; }
    public decimal ShippingFee { get; set; }
    public string CreatedBy { get; set; }

    // Cập nhật constructor
    public CreateOrderCommand(string userId, CreateOrderDto dto, string createdBy)
    {
        UserId = userId;
        SelectedItems = dto.SelectedItems; // Gán giá trị mới
        PaymentMethod = dto.PaymentMethod;
        CustomerName = dto.CustomerName;
        CustomerPhone = dto.CustomerPhone;
        ShippingAddress = dto.ShippingAddress;
        CustomerEmail = dto.CustomerEmail;
        Notes = dto.Notes;
        ShippingFee = dto.ShippingFee;
        CreatedBy = createdBy;
    }
}

// Get Order By Id Query
public class GetOrderByIdQuery : IRequest<OrderDetailDto>
{
    public string OrderId { get; set; }
    public string UserId { get; set; }

    public GetOrderByIdQuery(string orderId, string userId)
    {
        OrderId = orderId;
        UserId = userId;
    }
}

// Get Orders For User Query
public class GetUserOrdersQuery : IRequest<PagedModel<OrderSummaryDto>>
{
    public string UserId { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    public GetUserOrdersQuery(string userId, int pageIndex = 0, int pageSize = 10)
    {
        UserId = userId;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}

// Get All Orders Query (for admin)
public class GetAllOrdersQuery : IRequest<PagedModel<OrderSummaryDto>>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public OrderStatus? StatusFilter { get; set; }

    public GetAllOrdersQuery(int pageIndex = 0, int pageSize = 10, OrderStatus? statusFilter = null)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        StatusFilter = statusFilter;
    }
}

// Update Order Status Command
public class UpdateOrderStatusCommand : IRequest<bool>
{
    public string OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateOrderStatusCommand(string orderId, OrderStatus newStatus, string updatedBy)
    {
        OrderId = orderId;
        NewStatus = newStatus;
        UpdatedBy = updatedBy;
    }
}

// Update Payment Status Command
public class UpdatePaymentStatusCommand : IRequest<bool>
{
    public string OrderId { get; set; }
    public PaymentStatus NewStatus { get; set; }
    public string UpdatedBy { get; set; }

    public UpdatePaymentStatusCommand(string orderId, PaymentStatus newStatus, string updatedBy)
    {
        OrderId = orderId;
        NewStatus = newStatus;
        UpdatedBy = updatedBy;
    }
}