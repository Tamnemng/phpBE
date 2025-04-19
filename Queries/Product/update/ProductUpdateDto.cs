using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ProductUpdateDto
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    public string ImageUrl { get; set; }
    
    // Add ImageBase64 property for Cloudinary upload
    public string ImageBase64 { get; set; }
    
    [Required]
    public IEnumerable<string> CategoriesCode { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    
    [Required]
    public string BrandCode { get; set; }
    
    public string UpdatedBy { get; set; }
    
    public UpdatePriceCommand Price { get; set; }
    
    public List<VariantGroupUpdateDto> Variants { get; set; }

    public List<string> GiftCodes { get; set; } = new List<string>();
    
    public string ShortDescription { get; set; }
}

public class VariantGroupUpdateDto
{
    [Required]
    public string OptionTitle { get; set; }
    
    [Required]
    public List<ProductVariantUpdateDto> Options { get; set; }
}

public class ProductVariantUpdateDto
{
    [Required]
    public string OptionLabel { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal? OriginalPrice { get; set; }
    
    public decimal? CurrentPrice { get; set; }
    
    public int? Barcode { get; set; }
    
    public IEnumerable<Description> Descriptions { get; set; }
    
    public IEnumerable<Image> Images { get; set; }
    
    // Add support for base64 encoded images
    public IEnumerable<ImageBase64Dto> ImagesBase64 { get; set; }
    
    public string ShortDescription { get; set; }
}