
using MediatR;
using Dapr.Client;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCT_LIST_KEY = "product_list";
    private const string CATEGORY_PREFIX = "category_";

    public DeleteProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _daprClient.GetStateAsync<AddProductCommand>(STORE_NAME, request.Id, cancellationToken: cancellationToken);
        if (product != null)
        {
            var categoryKey = $"{CATEGORY_PREFIX}{product.CategoryId}";

            var categoryProducts = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, categoryKey, cancellationToken: cancellationToken) ?? new List<string>();
            if (categoryProducts.Contains(request.Id))
            {
                categoryProducts.Remove(request.Id);
                await _daprClient.SaveStateAsync(STORE_NAME, categoryKey, categoryProducts, cancellationToken: cancellationToken);
            }
        }

        var products = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, PRODUCT_LIST_KEY, cancellationToken: cancellationToken) ?? new List<string>();
        if (products.Contains(request.Id))
        {
            products.Remove(request.Id);
            await _daprClient.SaveStateAsync(STORE_NAME, PRODUCT_LIST_KEY, products, cancellationToken: cancellationToken);
        }

        await _daprClient.DeleteStateAsync(STORE_NAME, request.Id, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
