using MediatR;
using OMS.Core.Queries;

public enum ProductSortOption
{
    None,
    PriceAscending,
    PriceDescending,
    DiscountPercentageAscending,
    DiscountPercentageDescending,
    NameAscending,
    NameDescending
}

public class GetAllProductsQuery : IRequest<PagedModel<ProductSummaryDto>>
{
    public string? ProductName { get; }
    public string? BrandCode { get; }
    public string? CategoryCode { get; }
    public decimal? MinPrice { get; }
    public decimal? MaxPrice { get; }
    public decimal? MinDiscountPercentage { get; }
    public ProductSortOption SortBy { get; }
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllProductsQuery(
        string? productName = null, 
        string? brandCode = null, 
        string? categoryCode = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        decimal? minDiscountPercentage = null,
        ProductSortOption sortBy = ProductSortOption.None,
        int pageIndex = 0, 
        int pageSize = 10)
    {
        ProductName = productName;
        BrandCode = brandCode;
        CategoryCode = categoryCode;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        MinDiscountPercentage = minDiscountPercentage;
        SortBy = sortBy;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}