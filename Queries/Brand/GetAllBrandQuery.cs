
using MediatR;
using OMS.Core.Queries;

public class GetAllBrandQuery : IRequest<PagedModel<Brand>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllBrandQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}