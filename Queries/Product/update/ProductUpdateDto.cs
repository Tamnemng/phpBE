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
    
    // Required for image updates
    public string ImageBase64 { get; set; }
    
    [Required]
    public IEnumerable<string> CategoriesCode { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    
    [Required]
    public string BrandCode { get; set; }
    
    public UpdatePriceCommand Price { get; set; }
    
    public List<VariantGroupUpdateDto> Variants { get; set; }

    public List<string> GiftCodes { get; set; } = new List<string>();
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
    
    public decimal? OriginalPrice { get; set; }
    
    public decimal? CurrentPrice { get; set; }
    
    public IEnumerable<Description> Descriptions { get; set; }
    
    // Remove Images collection that accepts URLs
    // Only accept Base64 encoded images for updates
    public IEnumerable<ImageBase64Dto> ImagesBase64 { get; set; }
    
    public string ShortDescription { get; set; }
}