using MediatR;
using Dapr.Client;


public class GetProductsByCategoryHandler : IRequestHandler<GetProductsByCategoryQuery, List<AddProductCommand>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CATEGORY_PREFIX = "category_";

    public GetProductsByCategoryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<List<AddProductCommand>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var categoryKey = $"{CATEGORY_PREFIX}{request.CategoryId}";
        Console.WriteLine($"[DEBUG] Fetching category: {categoryKey}");

        var productIds = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, categoryKey, cancellationToken: cancellationToken)
                        ?? new List<string>();

        Console.WriteLine($"[DEBUG] Product IDs in category '{categoryKey}': {string.Join(", ", productIds)}");

        var products = new List<AddProductCommand>();

        foreach (var productId in productIds)
        {
            Console.WriteLine($"[DEBUG] Fetching product with ID: {productId}");
            var product = await _daprClient.GetStateAsync<AddProductCommand>(STORE_NAME, productId, cancellationToken: cancellationToken);
            if (product != null)
            {
                products.Add(product);
            }
        }

        Console.WriteLine($"[DEBUG] Total products found: {products.Count}");
        return products;
    }

}
