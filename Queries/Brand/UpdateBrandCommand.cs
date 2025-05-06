using MediatR;

public class UpdateBrandCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string UpdatedBy { get; set; }
    
    public UpdateBrandCommand( string id, string name, string image, string updatedBy)
    {
        Id = id;
        Name = name;
        Image = image;
        UpdatedBy = updatedBy;
    }
}
