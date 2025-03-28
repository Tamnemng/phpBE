using MediatR;

public class UpdateCategoryCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string UpdatedBy { get; set; }
    
    public UpdateCategoryCommand(string id, string name, string updatedBy)
    {
        Id = id;
        Name = name;
        UpdatedBy = updatedBy;
    }
}
