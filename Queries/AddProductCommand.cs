using MediatR;

public class AddProductCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Labels { get; set; }

    public AddProductCommand(string id, string name, Dictionary<string, object> labels)
    {
        Id = id;
        Name = name;
        Labels = labels;
    }
}
