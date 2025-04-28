// Queries/Product/GetRelatedProductsQuery.cs
using MediatR;
using System.Collections.Generic;

public class GetRelatedProductsQuery : IRequest<List<ProductSummaryDto>>
{
    public string ProductCode { get; }
    public int Count { get; }

    public GetRelatedProductsQuery(string productCode, int count)
    {
        ProductCode = productCode;
        Count = count;
    }
}