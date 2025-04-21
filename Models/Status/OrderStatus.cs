public enum OrderStatus
{
    Pending,           // Đơn hàng đã tạo nhưng chưa thanh toán
    Confirmed,         // Đơn hàng đã xác nhận thanh toán (hoặc COD)
    Processing,        // Đang chuẩn bị hàng
    Shipping,          // Đang giao hàng
    Delivered,         // Đã giao hàng thành công
    Canceled,          // Đã hủy
    Completed          // Đơn hàng đã hoàn thành (sau khi xác nhận đã nhận)
}

public enum PaymentMethod
{
    COD,                // Thanh toán khi nhận hàng
    OnlinePayment       // Thanh toán online
}

public enum PaymentStatus
{
    Pending,            // Chờ thanh toán
    Paid,               // Đã thanh toán
    Failed,             // Thanh toán thất bại
    Refunded            // Đã hoàn tiền
}