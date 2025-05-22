using MediatR;
using System.Collections.Generic;

public class GetBrandsByCategoryQuery : IRequest<List<BrandNameCodeDto>>
{
    public string CategoryCode { get; }

    public GetBrandsByCategoryQuery(string categoryCode)
    {
        CategoryCode = categoryCode;
    }
}