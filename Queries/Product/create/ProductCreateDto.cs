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
    
    [Required]
    public IEnumerable<string> CategoriesCode { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    
    [Required]
    public string BrandCode { get; set; }
    
    public string CreatedBy { get; set; }
    
    [Required]
    public List<ProductVariantDto> Variants { get; set; }

    public List<string> GiftCodes { get; set; } = new List<string>();
}

public class ProductVariantDto
{
    [Required]
    public string OptionTitle { get; set; } // Ví dụ: "Color"
    
    [Required]
    public string OptionLabel { get; set; } // Ví dụ: "Red"
    
    public int Quantity { get; set; } = 0;
    
    [Required]
    public decimal OriginalPrice { get; set; }
    
    [Required]
    public decimal CurrentPrice { get; set; }
    
    public int Barcode { get; set; }
    
    public IEnumerable<Description> Descriptions { get; set; }
    
    public IEnumerable<Image> Images { get; set; }
    
    public string ShortDescription { get; set; }
}