using MediatR;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Command for updating product brand
public class UpdateProductBrandCommand : IRequest<Unit>
{
    public string ProductCode { get; set; }
    public string BrandCode { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductBrandCommand(string productCode, string brandCode, string updatedBy)
    {
        ProductCode = productCode;
        BrandCode = brandCode;
        UpdatedBy = updatedBy;
    }
}

// Command for updating product categories
public class UpdateProductCategoriesCommand : IRequest<Unit>
{
    public string ProductCode { get; set; }
    public IEnumerable<string> CategoriesCode { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductCategoriesCommand(string productCode, IEnumerable<string> categoriesCode, string updatedBy)
    {
        ProductCode = productCode;
        CategoriesCode = categoriesCode;
        UpdatedBy = updatedBy;
    }
}

// Command for updating product main image
public class UpdateProductImageCommand : IRequest<Unit>
{
    public string ProductCode { get; set; }
    public string ImageUrl { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductImageCommand(string productCode, string imageUrl, string updatedBy)
    {
        ProductCode = productCode;
        ImageUrl = imageUrl;
        UpdatedBy = updatedBy;
    }
}

// Command for updating variant descriptions
public class UpdateProductDescriptionsCommand : IRequest<Unit>
{
    public string ProductId { get; set; }
    public IEnumerable<Description> Descriptions { get; set; }
    public string ShortDescription { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductDescriptionsCommand(string productId, IEnumerable<Description> descriptions, string shortDescription, string updatedBy)
    {
        ProductId = productId;
        Descriptions = descriptions;
        ShortDescription = shortDescription;
        UpdatedBy = updatedBy;
    }
}

// Command for updating product status
public class UpdateProductStatusByIdCommand : IRequest<Unit>
{
    public string ProductId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductStatusByIdCommand(string productId, ProductStatus status, string updatedBy)
    {
        ProductId = productId;
        Status = status;
        UpdatedBy = updatedBy;
    }
}

// Command for updating product status by code

public class UpdateProductStatusByCodeCommand : IRequest<Unit>
{
    public string ProductCode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductStatusByCodeCommand(string productCode, ProductStatus status, string updatedBy)
    {
        ProductCode = productCode;
        Status = status;
        UpdatedBy = updatedBy;
    }
}

// Command for updating variant price
public class UpdateVariantPriceCommand : IRequest<Unit>
{
    public string ProductId { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateVariantPriceCommand( string productId, decimal originalPrice, decimal currentPrice, string updatedBy)
    {
        ProductId = productId;
        OriginalPrice = originalPrice;
        CurrentPrice = currentPrice;
        UpdatedBy = updatedBy;
    }
}

// Command for updating product name
public class UpdateProductNameCommand : IRequest<Unit>
{
    public string ProductCode { get; set; }
    public string Name { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductNameCommand(string productCode, string name, string updatedBy)
    {
        ProductCode = productCode;
        Name = name;
        UpdatedBy = updatedBy;
    }
}

// Command for updating product gifts
public class UpdateProductGiftsCommand : IRequest<Unit>
{
    public string ProductCode { get; set; }
    public List<string> GiftCodes { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProductGiftsCommand(string productCode, List<string> giftCodes, string updatedBy)
    {
        ProductCode = productCode;
        GiftCodes = giftCodes;
        UpdatedBy = updatedBy;
    }
}