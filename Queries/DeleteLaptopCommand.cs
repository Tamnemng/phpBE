using MediatR;

public class DeleteLaptopCommand : IRequest<Unit>
{
    public string Id { get; set; }

    public DeleteLaptopCommand(string id)
    {
        Id = id;
    }
}