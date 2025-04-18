using MediatR;
using OMS.Core.Queries;

public class GetAllProductsQuery : IRequest<PagedModel<ProductSummaryDto>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllProductsQuery(int pageIndex = 0, int pageSize = 10)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}