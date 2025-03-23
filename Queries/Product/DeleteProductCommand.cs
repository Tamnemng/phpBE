using MediatR;

public class DeleteProductCommand : IRequest<bool>
{
    public string Id { get; set; }

    public DeleteProductCommand(string id)
    {
        Id = id;
    }
}