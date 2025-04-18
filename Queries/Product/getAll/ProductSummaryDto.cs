public class ProductSummaryDto
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string BrandName { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public string ShortDescription { get; set; }

    public ProductSummaryDto()
    {
        Id = string.Empty;
        Code = string.Empty;
        Name = string.Empty;
        ImageUrl = string.Empty;
        BrandName = string.Empty;
        ShortDescription = string.Empty;
    }

    public ProductSummaryDto(Product product, string brandName)
    {
        Id = product.ProductInfo.Id;
        Code = product.ProductInfo.Code;
        Name = product.ProductInfo.Name;
        ImageUrl = product.ProductInfo.ImageUrl;
        BrandName = brandName;
        CurrentPrice = product.Price.CurrentPrice;
        OriginalPrice = product.Price.OriginalPrice;
        ShortDescription = product.ProductDetail.ShortDescription;
    }
}