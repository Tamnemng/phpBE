
using MediatR;
using Dapr.Client;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    
    public DeleteProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken) 
            ?? new List<Product>();
        var productToRemove = products.FirstOrDefault(p => p.Id == request.Id);
        if (productToRemove == null)
        {
            return false;
        }
        
        products.Remove(productToRemove);
        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);
        return true;
    }
}