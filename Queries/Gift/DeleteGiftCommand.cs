using MediatR;

public class DeleteGiftCommand : IRequest<bool>
{
    public IEnumerable<string> Id { get; set; }

    public DeleteGiftCommand(IEnumerable<string> id)
    {
        Id = id;
    }
}