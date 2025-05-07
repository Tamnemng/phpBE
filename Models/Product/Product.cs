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
            variant.Descriptions,
            variant.Images,
            variant.ShortDescription
        );

        ProductOptions = new List<ProductOption>();
    }

    public void UpdateProductBrand(string brandCode, string updatedBy)
    {
        base.Update(updatedBy);
        ProductInfo.Brand = brandCode;
    }

    public void UpdateProductCategory(IEnumerable<string> categoryCode, string updatedBy)
    {
        base.Update(updatedBy);
        ProductInfo.Category = categoryCode;
    }

    public void UpdateProductImage(string imageUrl, string updatedBy)
    {
        base.Update(updatedBy);
        ProductInfo.ImageUrl = imageUrl;
    }

    public void UpdateProductStatus(ProductStatus status, string updatedBy)
    {
        base.Update(updatedBy);
        ProductInfo.Status = status;
    }

    public void UpdateProductPrice(decimal originalPrice, decimal currentPrice, string updatedBy)
    {
        base.Update(updatedBy);
        Price.Update(originalPrice, currentPrice);
    }

    public void UpdateProductName(string name, string updatedBy)
    {
        base.Update(updatedBy);
        ProductInfo.Name = name;
    }

    public void UpdateProductDescription(string shortDescription, IEnumerable<Description> descriptions, string updatedBy)
    {
        base.Update(updatedBy);
        ProductDetail.ShortDescription = shortDescription;
        ProductDetail.Description = descriptions;

    }

    public void UpdateProductGifts(List<Gift> gifts, string updatedBy)
    {
        base.Update(updatedBy);
        Gifts = gifts;
    }

    // public void Update(UpdateProductCommand command)
    // {
    //     base.Update(command.UpdatedBy);

    //     ProductInfo.Name = command.Name;
    //     ProductInfo.ImageUrl = command.ImageUrl;
    //     ProductInfo.Status = command.Status;
    //     ProductInfo.Brand = command.BrandCode;
    //     ProductInfo.Category = command.CategoriesCode;

    //     if (command.Price != null)
    //     {
    //         Price.Update(command.Price);
    //     }

    //     if (!string.IsNullOrEmpty(command.ShortDescription))
    //     {
    //         ProductDetail.ShortDescription = command.ShortDescription;
    //     }

    //     if (command.Gifts != null && command.Gifts.Any())
    //     {
    //         Gifts = command.Gifts;
    //     }
    // }
}