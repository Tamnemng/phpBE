
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
        var brands = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, KEY, cancellationToken: cancellationToken) 
            ?? new List<Product>();
        var brand = brands.FirstOrDefault(p => request.Id.Contains(p.ProductInfo.Id));
        if (brand == null)
        {
            return false;
        }
        
        brands.Remove(brand);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, brands, cancellationToken: cancellationToken);
        return true;
    }
}