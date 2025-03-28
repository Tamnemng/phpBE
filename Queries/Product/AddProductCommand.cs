using MediatR;

public class AddProductCommand : IRequest<Unit>
{
    
    public string Name { get; set;}
    public string Code { get; set;}
    public string ImageUrl { get; set; }
    public IEnumerable<Category> Categories { get; set; }
    public ProductStatus Status { get; set; }
    public Brand Brand { get; set; }
    public string CreatedBy { get; set;}

    public AddProductCommand(string name, string code, string imageUrl, ProductStatus status, IEnumerable<Category> categories, Brand brand, string createdBy)
    {
        Name = name;
        Code = code;
        ImageUrl = imageUrl;
        Categories = categories;
        Brand = brand;
        Status = status;
        CreatedBy = createdBy;
    }
}