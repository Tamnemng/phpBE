
using MediatR;
using OMS.Core.Queries;

public class GetAllLaptopQuery : IRequest<PagedModel<KeyValuePair<string, string>>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllLaptopQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}