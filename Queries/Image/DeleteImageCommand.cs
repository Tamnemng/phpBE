using MediatR;
public class DeleteImageCollectionCommand : IRequest<bool>
{
    public string Id { get; }

    public DeleteImageCollectionCommand(string id)
    {
        Id = id;
    }
}