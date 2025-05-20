// Handlers/Order/CreateOrderHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDetailDto>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string ORDERS_KEY = "orders";
    private const string CART_KEY = "carts";
    private const string PRODUCTS_KEY = "products";
    private const string COMBOS_KEY = "combos";

    public CreateOrderHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<OrderDetailDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Get user's cart
        var carts = await _daprClient.GetStateAsync<List<Cart>>(
            STORE_NAME,
            CART_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Cart>();
        
        var userCart = carts.FirstOrDefault(c => c.UserId == command.UserId);
        
        if (userCart == null || !userCart.Items.Any())
        {
            throw new InvalidOperationException("Cannot create order because the cart is empty.");
        }
        
        // Get products and combos to calculate prices
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        // Create order items from cart items
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;
        
        foreach (var cartItem in userCart.Items)
        {
            if (cartItem.ItemType == CartItemType.Product)
            {
                var product = products.FirstOrDefault(p => p.ProductInfo.Id == cartItem.ItemId);
                if (product != null)
                {
                    var orderItem = new OrderItem(
                        product.ProductInfo.Id,
                        CartItemType.Product,
                        product.ProductInfo.Name,
                        product.ProductInfo.ImageUrl,
                        cartItem.Quantity,
                        product.Price.CurrentPrice
                    );
                    
                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
            }
            else if (cartItem.ItemType == CartItemType.Combo)
            {
                var combo = combos.FirstOrDefault(c => c.Id == cartItem.ItemId);
                if (combo != null)
                {
                    var orderItem = new OrderItem(
                        combo.Id,
                        CartItemType.Combo,
                        combo.Name,
                        combo.ImageUrl,
                        cartItem.Quantity,
                        combo.ComboPrice
                    );
                    
                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
            }
        }
        
        if (!orderItems.Any())
        {
            throw new InvalidOperationException("Cannot create order because no valid items were found in the cart.");
        }
        
        // Create order
        var order = new Order(
            command.UserId,
            orderItems,
            totalAmount,
            command.ShippingFee,
            command.PaymentMethod,
            command.CustomerName,
            command.CustomerPhone,
            command.ShippingAddress,
            command.CustomerEmail,
            command.Notes,
            command.CreatedBy
        );
        
        // Save order
        var orders = await _daprClient.GetStateAsync<List<Order>>(
            STORE_NAME,
            ORDERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Order>();
        
        orders.Add(order);
        
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            ORDERS_KEY,
            orders,
            cancellationToken: cancellationToken
        );
        
        // Clear user's cart
        userCart.Items.Clear();
        
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            CART_KEY,
            carts,
            cancellationToken: cancellationToken
        );
        
        // If payment method is COD, automatically set status to confirmed
        if (order.PaymentMethod == PaymentMethod.COD)
        {
            order.UpdateStatus(OrderStatus.Confirmed, command.CreatedBy);
        }
        
        return new OrderDetailDto(order);
    }
}