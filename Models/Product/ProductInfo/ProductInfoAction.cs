using System.ComponentModel.DataAnnotations;
using Google.Rpc;

public class AddProductDto
{
    public string Code { get; set; }

    public string Name { get; set; }

    public string ImageUrl { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.InStock;
    public IEnumerable<Category> Category { get; set; }

    public Brand Brand { get; set; }

    public AddProductDto()
    {
        Code = string.Empty;
        Name = string.Empty;
        Category = new List<Category>();
        Brand = new Brand();
        ImageUrl = string.Empty;
    }

    public AddProductDto(string name, string code, IEnumerable<Category> categories, Brand brand, string imageUrl)
    {
        Code = code;
        Name = name;
        Category = categories;
        Brand = brand;
        ImageUrl = imageUrl;
    }
}

public class UpdateProductDto
{
    public string Name { get; set; }

    public string ImageUrl { get; set; }

    public ProductStatus Status { get; set; }

    public IEnumerable<Category> Category { get; set; }
    public Brand Brand { get; set; }

    public UpdateProductDto()
    {
        Name = string.Empty;
        Category = new List<Category>();
        Brand = new Brand();
        ImageUrl = string.Empty;
        Status = ProductStatus.New;
    }

    public UpdateProductDto(string name, IEnumerable<Category> categories, Brand brand, string imageUrl, ProductStatus status)
    {
        Name = name;
        Category = categories;
        Brand = brand;
        ImageUrl = imageUrl;
        Status = status;
    }
}