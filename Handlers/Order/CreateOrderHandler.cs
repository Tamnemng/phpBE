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
    private const string CART_KEY = "carts"; // Vẫn cần để xóa item khỏi giỏ hàng
    private const string PRODUCTS_KEY = "products";
    private const string COMBOS_KEY = "combos";

    public CreateOrderHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<OrderDetailDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        if (command.SelectedItems == null || !command.SelectedItems.Any())
        {
            throw new InvalidOperationException("Không thể tạo đơn hàng vì không có sản phẩm nào được chọn.");
        }
        
        // Get products and combos to calculate prices and verify existence
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
        
        var orderItems = new List<OrderItem>();
        decimal totalAmount = 0;
        
        // Create order items from selected items
        foreach (var selectedItem in command.SelectedItems)
        {
            if (selectedItem.ItemType == CartItemType.Product)
            {
                var product = products.FirstOrDefault(p => p.ProductInfo.Id == selectedItem.ItemId);
                if (product != null)
                {
                    // Kiểm tra xem sản phẩm có còn hoạt động/trong kho không (tuỳ theo logic của bạn)
                    if (product.ProductInfo.Status == ProductStatus.OutOfStock || product.ProductInfo.Status == ProductStatus.Pending) // Ví dụ
                    {
                        throw new InvalidOperationException($"Sản phẩm '{product.ProductInfo.Name}' không có sẵn để đặt hàng.");
                    }

                    var orderItem = new OrderItem(
                        product.ProductInfo.Id,
                        CartItemType.Product,
                        product.ProductInfo.Name,
                        product.ProductInfo.ImageUrl,
                        selectedItem.Quantity, // Lấy quantity từ selectedItem
                        product.Price.CurrentPrice
                    );
                    
                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
                else
                {
                    throw new InvalidOperationException($"Sản phẩm với ID '{selectedItem.ItemId}' không tồn tại.");
                }
            }
            else if (selectedItem.ItemType == CartItemType.Combo)
            {
                var combo = combos.FirstOrDefault(c => c.Id == selectedItem.ItemId);
                if (combo != null)
                {
                    // Kiểm tra xem combo có còn hoạt động không
                    if (!combo.IsActive)
                    {
                        throw new InvalidOperationException($"Combo '{combo.Name}' không có sẵn để đặt hàng.");
                    }

                    var orderItem = new OrderItem(
                        combo.Id,
                        CartItemType.Combo,
                        combo.Name,
                        combo.ImageUrl,
                        selectedItem.Quantity, // Lấy quantity từ selectedItem
                        combo.ComboPrice
                    );
                    
                    orderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
                else
                {
                     throw new InvalidOperationException($"Combo với ID '{selectedItem.ItemId}' không tồn tại.");
                }
            }
        }
        
        if (!orderItems.Any())
        {
            // Điều này không nên xảy ra nếu logic ở trên đúng, nhưng là một biện pháp phòng ngừa
            throw new InvalidOperationException("Không thể tạo đơn hàng vì không có sản phẩm hợp lệ nào được xử lý.");
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
        
        // Get user's cart to remove ordered items
        var carts = await _daprClient.GetStateAsync<List<Cart>>(
            STORE_NAME,
            CART_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Cart>();
        
        var userCart = carts.FirstOrDefault(c => c.UserId == command.UserId);
        if (userCart != null)
        {
            foreach (var orderedItemInfo in command.SelectedItems)
            {
                // Tìm và xóa item tương ứng khỏi giỏ hàng
                // Lưu ý: Logic `RemoveItem` trong `Cart.cs` cần đảm bảo xử lý đúng
                userCart.RemoveItem(orderedItemInfo.ItemId, orderedItemInfo.ItemType);
            }
            
            await _daprClient.SaveStateAsync(
                STORE_NAME,
                CART_KEY,
                carts, // Lưu lại danh sách carts đã được cập nhật
                cancellationToken: cancellationToken
            );
        }
        
        // If payment method is COD, automatically set status to confirmed
        if (order.PaymentMethod == PaymentMethod.COD)
        {
            order.UpdateStatus(OrderStatus.Confirmed, command.CreatedBy);
            // Cần lưu lại trạng thái order một lần nữa nếu có thay đổi
            await _daprClient.SaveStateAsync(STORE_NAME, ORDERS_KEY, orders, cancellationToken: cancellationToken);
        }
        
        return new OrderDetailDto(order);
    }
}