using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class AddComboDto
{
    [Required]
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    // For base64 encoded image upload
    public string ImageBase64 { get; set; }
    
    [Required]
    public List<string> ProductCodes { get; set; }
    
    [Required]
    public decimal ComboPrice { get; set; }
}

public class UpdateComboDto
{
    [Required]
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    // For base64 encoded image upload
    public string ImageBase64 { get; set; }
    
    public List<string> ProductCodes { get; set; }
    
    public decimal? ComboPrice { get; set; }
    
    public bool? IsActive { get; set; }
}

public class ComboDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<string> ProductCodes { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; }
    public List<ProductSummaryDto> Products { get; set; }
    
    public ComboDto()
    {
        Id = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
        ProductCodes = new List<string>();
        Products = new List<ProductSummaryDto>();
        IsActive = true;
    }
    
    public ComboDto(Combo combo, List<ProductSummaryDto> products = null)
    {
        Id = combo.Id;
        Name = combo.Name;
        Description = combo.Description;
        ImageUrl = combo.ImageUrl;
        ProductCodes = combo.ProductCodes;
        OriginalPrice = combo.OriginalPrice;
        ComboPrice = combo.ComboPrice;
        DiscountPercentage = combo.DiscountPercentage;
        IsActive = combo.IsActive;
        Products = products ?? new List<ProductSummaryDto>();
    }
}