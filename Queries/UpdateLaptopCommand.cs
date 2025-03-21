using MediatR;

public class UpdateLaptopCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Labels { get; set; }

    public UpdateLaptopCommand(string id, string name, Dictionary<string, object> labels)
    {
        Id = id;
        Name = name;
        Labels = labels;
    }
}
