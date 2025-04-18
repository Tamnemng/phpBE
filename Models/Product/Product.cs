public class Product : BaseEntity
{
    public ProductInfo ProductInfo { get; set; }
    public Price Price { get; set; }
    public ProductDetail ProductDetail { get; set; } = new ProductDetail();
    public List<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
    public List<Gift> Gifts { get; set; } = new List<Gift>();
    public Product()
    {
        ProductInfo = new ProductInfo();
        Price = new Price();
        ProductOptions = new List<ProductOption>();
    }
    public Product(AddProductCommand command, string productId, ProductVariant variant) : base(command.CreatedBy)
    {
        ProductInfo = new ProductInfo
        {
            Id = productId,
            Name = command.Name,
            Code = command.Code,
            ImageUrl = command.ImageUrl,
            Brand = command.BrandCode,
            Category = command.CategoriesCode,
            Status = command.Status
        };
        Gifts = command.Gifts;
        Price = Price.Create(variant.OriginalPrice, variant.CurrentPrice);
        ProductDetail = new ProductDetail(
            variant.Barcode,
            variant.Descriptions,
            variant.Images,
            variant.ShortDescription
        );
        
        ProductOptions = new List<ProductOption>();
    }

    public void Update(UpdateProductCommand command)
    {
        ProductInfo.Name = command.Name;
        ProductInfo.ImageUrl = command.ImageUrl;
        ProductInfo.Status = command.Status;
        ProductInfo.Brand = command.BrandId;
        ProductInfo.Category = command.CategoriesId;
    }
}