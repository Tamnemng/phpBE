using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ProductCreateDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Code { get; set; }
    
    public string ImageUrl { get; set; }
    
    // Add ImageBase64 property for Cloudinary upload
    public string ImageBase64 { get; set; }
    
    [Required]
    public IEnumerable<string> CategoriesCode { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    
    [Required]
    public string BrandCode { get; set; }
    
    public string CreatedBy { get; set; }
    
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
    
    public int Quantity { get; set; } = 0;
    
    [Required]
    public decimal OriginalPrice { get; set; }
    
    [Required]
    public decimal CurrentPrice { get; set; }
    
    public int Barcode { get; set; }
    
    public IEnumerable<Description> Descriptions { get; set; }
    
    public IEnumerable<Image> Images { get; set; }
    
    // Add support for base64 encoded images
    public IEnumerable<ImageBase64Dto> ImagesBase64 { get; set; }
    
    public string ShortDescription { get; set; }
}