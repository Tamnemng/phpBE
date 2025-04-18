using MediatR;

public class UpdateCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public string OldProductId { get; set; }
    public string NewProductId { get; set; }
    public int Quantity { get; set; }

    public UpdateCartCommand(string userId, string oldProductId, string newProductId, int quantity)
    {
        UserId = userId;
        OldProductId = oldProductId;
        NewProductId = newProductId;
        Quantity = quantity;
    }
}