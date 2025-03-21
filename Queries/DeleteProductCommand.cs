using MediatR;

public class DeleteProductCommand : IRequest<Unit>
{
    public string Id { get; set; }

    public DeleteProductCommand(string id)
    {
        Id = id;
    }
}