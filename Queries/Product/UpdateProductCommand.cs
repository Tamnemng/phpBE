using MediatR;

public class UpdateProductCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public IEnumerable<Category> Categories { get; set; }
    public string ImageUrl { get; set; }
    public ProductStatus Status { get; set;}
    public Brand Brand { get; set; }
    public string UpdatedBy { get; set; }
    
    public UpdateProductCommand(string id,string code , string name, IEnumerable<Category> categories,Brand brand, string imageUrl, ProductStatus status , string updatedBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Categories = categories;
        Brand = brand;
        ImageUrl = imageUrl;
        Status = status;
        UpdatedBy = updatedBy;
    }
}
