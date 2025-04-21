using MediatR;
using Dapr.Client;
public class GetImageCollectionByIdHandler : IRequestHandler<GetImageCollectionByIdQuery, ImageCollection>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string IMAGE_COLLECTIONS_KEY = "image_collections";

    public GetImageCollectionByIdHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<ImageCollection> Handle(GetImageCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        var collections = await _daprClient.GetStateAsync<List<ImageCollection>>(
            STORE_NAME,
            IMAGE_COLLECTIONS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<ImageCollection>();

        var collection = collections.FirstOrDefault(c => c.Id == request.Id);
        
        if (collection == null)
        {
            throw new InvalidOperationException($"ImageCollection with ID '{request.Id}' not found.");
        }

        return collection;
    }
}