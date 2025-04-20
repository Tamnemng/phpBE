using MediatR;
using System.Collections.Generic;

public class GetCombosByProductCodeQuery : IRequest<List<ComboDto>>
{
    public string ProductCode { get; }

    public GetCombosByProductCodeQuery(string productCode)
    {
        ProductCode = productCode;
    }
}