using MediatR;
using Dapr.Client;


public class GetProductsByCategoryHandler : IRequestHandler<GetProductsByCategoryQuery, List<Product>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public GetProductsByCategoryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<List<Product>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var allProducts = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken)
            ?? new List<Product>();

        var productsInCategory = allProducts
            .Where(p => p.CategoryId == request.CategoryId)
            .ToList();
        return productsInCategory;
    }
}