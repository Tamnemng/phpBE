using OMS.Core.Utilities;

public class ProductInfo
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public IEnumerable<string> Category { get; set; }
    public string Brand { get; set; }
    public ProductInfo()
    {
        Id = string.Empty;
        Name = string.Empty;
        Code = string.Empty;
        ImageUrl = string.Empty;
        Brand = string.Empty;
        Category = Enumerable.Empty<string>();
        Status = ProductStatus.New; 
    }

    public ProductInfo (AddProductDto command) {
        Id = IdGenerator.GenerateId(20);
        Name = command.Name;
        Code= command.Code;
        ImageUrl = command.ImageUrl;
        Brand = command.Brand;
        Category = command.Category;
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