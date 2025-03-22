using MediatR;

public class UpdateProductCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CategoryId { get; set; }
    public Dictionary<string, object> Labels { get; set; }

    public UpdateProductCommand(string id, string name, string categoryId, Dictionary<string, object> labels)
    {
        Id = id;
        Name = name;
        CategoryId = categoryId;
        Labels = labels;
    }
}
