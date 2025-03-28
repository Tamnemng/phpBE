
using MediatR;
using OMS.Core.Queries;

public class GetAllCategoryQuery : IRequest<PagedModel<Category>>
{
    public int PageIndex { get; }
    public int PageSize { get; }

    public GetAllCategoryQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}