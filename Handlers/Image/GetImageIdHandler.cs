using MediatR;
using Dapr.Client;
public class GetAllImageCollectionIdsHandler : IRequestHandler<GetAllImageCollectionIdsQuery, List<string>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string IMAGE_COLLECTIONS_KEY = "image_collections";

    public GetAllImageCollectionIdsHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<string>> Handle(GetAllImageCollectionIdsQuery request, CancellationToken cancellationToken)
    {
        var collections = await _daprClient.GetStateAsync<List<ImageCollection>>(
            STORE_NAME,
            IMAGE_COLLECTIONS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<ImageCollection>();

        return collections.Select(c => c.Id).ToList();
    }
}