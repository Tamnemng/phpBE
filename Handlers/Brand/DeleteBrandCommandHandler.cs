
using MediatR;
using Dapr.Client;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "brands";
    
    public DeleteBrandCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    public async Task<bool> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(STORE_NAME, KEY, cancellationToken: cancellationToken) 
            ?? new List<BrandMetaData>();
        var brand = brands.FirstOrDefault(p => request.Id.Contains(p.Id));
        if (brand == null)
        {
            return false;
        }
        
        brands.Remove(brand);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, brands, cancellationToken: cancellationToken);
        return true;
    }
}