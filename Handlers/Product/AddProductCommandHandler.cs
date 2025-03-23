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
    private const string PRODUCTS_KEY = "products";

    public AddProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        string productId = GenerateUniqueId(16);
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken) 
            ?? new List<Product>();

        while (products.Any(p => p.Id == productId))
        {
            productId = GenerateUniqueId(16);
        }

        var now = DateTime.UtcNow;
        products.Add(new Product
        {
            Id = productId,
            Name = request.Name,
            CategoryId = request.CategoryId,
            Labels = request.Labels,
            CreatedDate = now,
            CreatedBy = request.CreatedBy,
            UpdatedDate = null,
            UpdatedBy = null
        });
        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);
        
        return Unit.Value;
    }
    private string GenerateUniqueId(int length)
    {
        string timestamp = DateTime.UtcNow.Ticks.ToString();
        string guid = Guid.NewGuid().ToString("N");
        string combined = guid + timestamp;
        if (combined.Length > length)
        {
            return combined.Substring(0, length);
        }
        else
        {
            return combined.PadRight(length, 'X');
        }
    }
}