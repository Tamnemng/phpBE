public class ProductVariantsDto
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string MainImageUrl { get; set; }
    public List<VariantGroupSummaryDto> VariantGroups { get; set; } = new List<VariantGroupSummaryDto>();
    public List<ProductVariantDetailDto> Combinations { get; set; } = new List<ProductVariantDetailDto>();
}

// Sử dụng tên lớp khác để tránh xung đột
public class VariantGroupSummaryDto
{
    public string OptionTitle { get; set; }
    public List<string> Options { get; set; } = new List<string>();
}

public class ProductVariantDetailDto
{
    public string Id { get; set; }
    public Dictionary<string, string> SelectedOptions { get; set; } = new Dictionary<string, string>();
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int Quantity { get; set; }
    public List<string> ImageUrls { get; set; } = new List<string>();
}