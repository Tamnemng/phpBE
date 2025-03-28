using OMS.Core.Utilities;

public class ProductInfo
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public IEnumerable<Category> Category { get; set; }
    public Brand Brand { get; set; }
    public ProductInfo()
    {
        Id = string.Empty;
        Name = string.Empty;
        Code = string.Empty;
        ImageUrl = string.Empty;
        Brand = new Brand();
        Category = Enumerable.Empty<Category>();
        Status = ProductStatus.New; 
    }

    public ProductInfo (AddProductDto command) {
        Id = IdGenerator.GenerateId(20);
        Name = command.Name;
        Code= command.Code;
        ImageUrl = command.ImageUrl;
        Brand = new Brand 
        { 
            Name = command.Brand.Name,
            Code = command.Brand.Code,
            Logo = command.Brand.Logo
        };

        Category = command.Category.Select(c => new Category 
        { 
            Name = c.Name,
            Code = c.Code
        });
        Status = command.Status;
    }

    public void Update(UpdateProductDto command) {
        Name = command.Name;
        ImageUrl = command.ImageUrl;
        Category= command.Category;
        Brand = command.Brand;
        Status = command.Status;
    }

}