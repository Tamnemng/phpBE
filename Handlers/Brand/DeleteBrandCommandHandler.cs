
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

        var existingIds = brands.Select(b => b.Id).ToList();
        var nonExistingIds = request.Id.Where(id => !existingIds.Contains(id)).ToList();

        if (nonExistingIds.Any())
        {
            throw new InvalidOperationException($"Không tìm thấy thương hiệu với ID: {string.Join(", ", nonExistingIds)}");
        }

        var initialCount = brands.Count;
        brands.RemoveAll(brand => request.Id.Contains(brand.Id));

        if (brands.Count == initialCount)
        {
            return false; // No brands were removed
        }
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, brands, cancellationToken: cancellationToken);
        return true;
    }
}