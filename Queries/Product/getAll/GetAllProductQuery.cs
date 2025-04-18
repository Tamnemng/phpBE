using MediatR;
using OMS.Core.Queries;

public class GetAllProductsQuery : IRequest<PagedModel<ProductSummaryDto>>
{
    public string? ProductName { get; }
    public string? BrandCode { get; }
    public string? CategoryCode { get; }
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllProductsQuery(string? productName = null, string? brandCode = null, string? categoryCode = null, int pageIndex = 0, int pageSize = 10)
    {
        ProductName = productName;
        BrandCode = brandCode;
        CategoryCode = categoryCode;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}