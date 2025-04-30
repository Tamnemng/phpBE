using MediatR;
using System.Collections.Generic;

public class GetProductVariantsQuery : IRequest<ProductVariantsDto>
{
    public string ProductCode { get; }

    public GetProductVariantsQuery(string productCode)
    {
        ProductCode = productCode;
    }
}
