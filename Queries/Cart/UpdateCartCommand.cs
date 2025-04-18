using MediatR;

public class UpdateCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }

    public UpdateCartCommand(string userId, string productId, int quantity)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        Quantity = quantity;
    }
}