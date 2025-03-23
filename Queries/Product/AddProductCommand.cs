using MediatR;

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CategoryId { get; set; }
    public Dictionary<string, object> Labels { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
}

public class AddProductCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string CategoryId { get; set; }
    public Dictionary<string, object> Labels { get; set; }
    public string CreatedBy { get; set; }

    public AddProductCommand(string name, string categoryId, Dictionary<string, object> labels, string createdBy)
    {
        Name = name;
        CategoryId = categoryId;
        Labels = labels;
        CreatedBy = createdBy;
    }
}