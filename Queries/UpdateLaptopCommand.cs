using MediatR;

public class UpdateLaptopCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string NewValue { get; }
    public UpdateLaptopCommand(string id, string newValue)
    {
        Id = id;
        NewValue = newValue;
    }
}