// Models/Order/Order.cs
using OMS.Core.Utilities;
using System;
using System.Collections.Generic;

public class Order : BaseEntity
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string OrderNumber { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal FinalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    
    // Customer information
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public string ShippingAddress { get; set; }
    public string CustomerEmail { get; set; }
    
    // Tracking information
    public string Notes { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public DateTime? ProcessingDate { get; set; }
    public DateTime? ShippingDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CanceledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public Order() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Items = new List<OrderItem>();
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
        OrderNumber = GenerateOrderNumber();
    }

    public Order(string userId, List<OrderItem> items, decimal totalAmount, decimal shippingFee, 
               PaymentMethod paymentMethod, string customerName, string customerPhone, string shippingAddress, 
               string customerEmail, string notes, string createdBy) : base(createdBy)
    {
        Id = IdGenerator.GenerateId(16);
        UserId = userId;
        Items = items;
        TotalAmount = totalAmount;
        ShippingFee = shippingFee;
        FinalAmount = totalAmount + shippingFee;
        Status = OrderStatus.Pending;
        PaymentMethod = paymentMethod;
        PaymentStatus = paymentMethod == PaymentMethod.COD ? PaymentStatus.Pending : PaymentStatus.Pending;
        CustomerName = customerName;
        CustomerPhone = customerPhone;
        ShippingAddress = shippingAddress;
        CustomerEmail = customerEmail;
        Notes = notes;
        OrderNumber = GenerateOrderNumber();
    }
    
    private string GenerateOrderNumber()
    {
        // Format: ORD-yyyyMMdd-randomDigits
        return $"ORD-{DateTime.Now:yyyyMMdd}-{IdGenerator.GenerateId(8)}";
    }
    
    public void UpdatePaymentStatus(PaymentStatus newStatus, string updatedBy)
    {
        PaymentStatus = newStatus;
        
        // If payment is successful and method is online payment, update order status
        if (newStatus == PaymentStatus.Paid && PaymentMethod == PaymentMethod.OnlinePayment)
        {
            UpdateStatus(OrderStatus.Confirmed, updatedBy);
        }
        
        base.Update(updatedBy);
    }
    
    public void UpdateStatus(OrderStatus newStatus, string updatedBy)
    {
        Status = newStatus;
        base.Update(updatedBy);
        
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                ConfirmedDate = DateTime.Now;
                break;
            case OrderStatus.Processing:
                ProcessingDate = DateTime.Now;
                break;
            case OrderStatus.Shipping:
                ShippingDate = DateTime.Now;
                break;
            case OrderStatus.Delivered:
                DeliveredDate = DateTime.Now;
                break;
            case OrderStatus.Canceled:
                CanceledDate = DateTime.Now;
                break;
            case OrderStatus.Completed:
                CompletedDate = DateTime.Now;
                break;
        }
    }
}

public class OrderItem
{
    public string ItemId { get; set; }
    public CartItemType ItemType { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    public OrderItem()
    {
        ItemId = string.Empty;
        Name = string.Empty;
        ImageUrl = string.Empty;
    }
    
    public OrderItem(string itemId, CartItemType itemType, string name, string imageUrl, int quantity, decimal unitPrice)
    {
        ItemId = itemId;
        ItemType = itemType;
        Name = name;
        ImageUrl = imageUrl;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
    }
}