using MediatR;
using Dapr.Client;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "products";
    
    public DeleteProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {

        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, KEY, cancellationToken: cancellationToken)
           ?? new List<Product>();

        var existingIds = products.Select(b => b.ProductInfo.Code).ToList();
        var nonExistingIds = request.Id.Where(id => !existingIds.Contains(id)).ToList();

        if (nonExistingIds.Any())
        {
            throw new InvalidOperationException($"Product not found with CODE: {string.Join(", ", nonExistingIds)}");
        }

        var initialCount = products.Count;
        products.RemoveAll(brand => request.Id.Contains(brand.ProductInfo.Code));

        if (products.Count == initialCount)
        {
            return false;
        }
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, products, cancellationToken: cancellationToken);
        return true;
    }
}