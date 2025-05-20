using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class CreateOrderDto
{
    [Required]
    public List<OrderItemSelectionDto> SelectedItems { get; set; } // Thay đổi ở đây

    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    [Required]
    public string CustomerName { get; set; }
    
    [Required]
    public string CustomerPhone { get; set; }
    
    [Required]
    public string ShippingAddress { get; set; }
    
    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; }
    
    public string Notes { get; set; }
    
    public decimal ShippingFee { get; set; } = 0;

    public CreateOrderDto() // Thêm constructor nếu cần
    {
        SelectedItems = new List<OrderItemSelectionDto>();
    }
}

public class UpdateOrderStatusDto
{
    [Required]
    public string OrderId { get; set; }
    
    [Required]
    public OrderStatus NewStatus { get; set; }
}

public class UpdatePaymentStatusDto
{
    [Required]
    public string OrderId { get; set; }
    
    [Required]
    public PaymentStatus NewStatus { get; set; }
}

public class OrderSummaryDto
{
    public string Id { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public DateTime CreatedDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal FinalAmount { get; set; }
    
    public OrderSummaryDto(Order order)
    {
        Id = order.Id;
        OrderNumber = order.OrderNumber;
        CustomerName = order.CustomerName;
        CreatedDate = order.CreatedDate;
        Status = order.Status;
        PaymentStatus = order.PaymentStatus;
        FinalAmount = order.FinalAmount;
    }
}

public class OrderDetailDto
{
    public string Id { get; set; }
    public string OrderNumber { get; set; }
    public string UserId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal FinalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string ShippingAddress { get; set; }
    public string CustomerEmail { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public DateTime? ProcessingDate { get; set; }
    public DateTime? ShippingDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CanceledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    public OrderDetailDto(Order order)
    {
        Id = order.Id;
        OrderNumber = order.OrderNumber;
        UserId = order.UserId;
        Items = order.Items.Select(i => new OrderItemDto(i)).ToList();
        TotalAmount = order.TotalAmount;
        ShippingFee = order.ShippingFee;
        FinalAmount = order.FinalAmount;
        Status = order.Status;
        PaymentMethod = order.PaymentMethod;
        PaymentStatus = order.PaymentStatus;
        CustomerName = order.CustomerName;
        CustomerPhone = order.CustomerPhone;
        ShippingAddress = order.ShippingAddress;
        CustomerEmail = order.CustomerEmail;
        Notes = order.Notes;
        CreatedDate = order.CreatedDate;
        ConfirmedDate = order.ConfirmedDate;
        ProcessingDate = order.ProcessingDate;
        ShippingDate = order.ShippingDate;
        DeliveredDate = order.DeliveredDate;
        CanceledDate = order.CanceledDate;
        CompletedDate = order.CompletedDate;
    }
}

public class OrderItemDto
{
    public string ItemId { get; set; }
    public CartItemType ItemType { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    public OrderItemDto(OrderItem item)
    {
        ItemId = item.ItemId;
        ItemType = item.ItemType;
        Name = item.Name;
        ImageUrl = item.ImageUrl;
        Quantity = item.Quantity;
        UnitPrice = item.UnitPrice;
        TotalPrice = item.TotalPrice;
    }
}