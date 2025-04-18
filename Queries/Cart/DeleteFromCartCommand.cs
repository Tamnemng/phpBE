using MediatR;

public class DeleteFromCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public string[] ProductIds { get; set; }

    public DeleteFromCartCommand(string userId, string[] productIds)
    {
        UserId = userId;
        ProductIds = productIds;
    }
}