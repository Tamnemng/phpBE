using MediatR;

public class AddToCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }

    public AddToCartCommand(string userId, string productId, int quantity)
    {
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
    }
}