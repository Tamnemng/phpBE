using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Product>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public GetProductByIdQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Product> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var product = products.FirstOrDefault(p => p.ProductInfo.Id == query.Id);
        
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{query.Id}' not found.");
        }

        return product;
    }
}