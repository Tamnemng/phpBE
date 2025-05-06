using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ProductCreateDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Code { get; set; }
    
    // Remove ImageUrl property and only use ImageBase64
    [Required]
    public string ImageBase64 { get; set; }
    
    [Required]
    public IEnumerable<string> CategoriesCode { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    
    [Required]
    public string BrandCode { get; set; }
    
    [Required]
    public List<VariantGroupDto> Variants { get; set; }

    public List<string> GiftCodes { get; set; } = new List<string>();
}

public class VariantGroupDto
{
    [Required]
    public string OptionTitle { get; set; } // Example: "Color"
    
    [Required]
    public List<ProductVariantDto> Options { get; set; }
}

public class ProductVariantDto
{
    [Required]
    public string OptionLabel { get; set; } // Example: "Red"
    
    [Required]
    public decimal OriginalPrice { get; set; }
    
    [Required]
    public decimal CurrentPrice { get; set; }
    
    public IEnumerable<Description> Descriptions { get; set; }
    
    public IEnumerable<ImageBase64Dto> ImagesBase64 { get; set; }
    
    public string ShortDescription { get; set; }
}