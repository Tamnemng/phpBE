using MediatR;

public class UpdateBrandCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Logo { get; set; }
    public string UpdatedBy { get; set; }
    
    public UpdateBrandCommand( string name, string id, string logo, string updatedBy)
    {
        Id = id;
        Name = name;
        Logo = logo;
        UpdatedBy = updatedBy;
    }
}
