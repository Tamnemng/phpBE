using MediatR;

public class DeleteBrandCommand : IRequest<bool>
{
    public IEnumerable<string> Id { get; set; }

    public DeleteBrandCommand(IEnumerable<string> id)
    {
        Id = id;
    }
}