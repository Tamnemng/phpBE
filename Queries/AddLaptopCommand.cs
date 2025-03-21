using MediatR;

public class AddLaptopCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Labels { get; set; }

    public AddLaptopCommand(string id, string name, Dictionary<string, object> labels)
    {
        Id = id;
        Name = name;
        Labels = labels;
    }
}
