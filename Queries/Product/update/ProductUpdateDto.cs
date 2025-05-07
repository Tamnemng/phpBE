using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// DTO for updating product brand
public class UpdateProductBrandDto
{
    [Required]
    public string BrandCode { get; set; }
}

// DTO for updating product categories
public class UpdateProductCategoriesDto
{
    [Required]
    public IEnumerable<string> CategoriesCode { get; set; }
}

// DTO for updating product main image
public class UpdateProductImageDto
{
    [Required]
    public string ImageBase64 { get; set; }
}

// DTO for updating variant images
public class UpdateVariantImagesDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one image is required")]
    public List<ImageBase64Dto> ImagesBase64 { get; set; }
}

// DTO for updating variant descriptions
public class UpdateVariantDescriptionsDto
{
    public IEnumerable<Description> Descriptions { get; set; }
    public string ShortDescription { get; set; }
}

// DTO for updating product status
public class UpdateProductStatusDto
{
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
}

// DTO for updating variant price
public class UpdateVariantPriceDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Original price must be a positive value")]
    public decimal OriginalPrice { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Current price must be a positive value")]
    public decimal CurrentPrice { get; set; }
}

// DTO for updating product name
public class UpdateProductNameDto
{
    [Required]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
    public string Name { get; set; }
}

// DTO for updating product gifts
public class UpdateProductGiftsDto
{
    public List<string> GiftCodes { get; set; } = new List<string>();
}