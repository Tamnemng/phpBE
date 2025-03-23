using MediatR;
using System;
using System.Collections.Generic;

public class GetProductByIdQuery : IRequest<Product>
{
    public string Id { get; }

    public GetProductByIdQuery(string id)
    {
        Id = id;
    }
}