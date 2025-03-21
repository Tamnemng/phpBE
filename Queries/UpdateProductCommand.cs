using MediatR;

public class UpdateProductCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, object> Labels { get; set; }

    public UpdateProductCommand(string id, string name, Dictionary<string, object> labels)
    {
        Id = id;
        Name = name;
        Labels = labels;
    }
}
