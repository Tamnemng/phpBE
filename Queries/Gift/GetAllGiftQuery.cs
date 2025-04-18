
using MediatR;
using OMS.Core.Queries;

public class GetAllGiftQuery : IRequest<PagedModel<Gift>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllGiftQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}