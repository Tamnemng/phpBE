
using MediatR;
using OMS.Core.Queries;

public class GetAllProductQuery : IRequest<PagedModel<KeyValuePair<string, string>>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllProductQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}