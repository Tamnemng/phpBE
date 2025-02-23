using MediatR;
using OMS.Core.Queries;

public class GetAllValuesQuery : IRequest<PagedModel<KeyValuePair<string, string>>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllValuesQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}
