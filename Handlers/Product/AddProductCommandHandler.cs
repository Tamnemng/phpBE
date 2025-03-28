using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "products";

    public AddProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        var brands = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, KEY, cancellationToken: cancellationToken)
            ?? new List<Product>();
        if (brands.Any(b => b.ProductInfo.Code.Equals(command.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A product with code '{command.Code}' already exists.");
        }
        var brand = new Product(command);
        brands.Add(brand);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, brands, cancellationToken: cancellationToken);
        return Unit.Value;
    }

}