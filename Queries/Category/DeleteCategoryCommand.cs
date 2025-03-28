using MediatR;

public class DeleteCategoryCommand : IRequest<bool>
{
    public IEnumerable<string> Id { get; set; }

    public DeleteCategoryCommand(IEnumerable<string> id)
    {
        Id = id;
    }
}