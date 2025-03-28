using MediatR;

public class DeleteProductCommand : IRequest<bool>
{
    public IEnumerable<string> Id { get; set; }

    public DeleteProductCommand(IEnumerable<string> id)
    {
        Id = id;
    }
}