using MediatR;
using Dapr.Client;
public class DeleteImageCollectionHandler : IRequestHandler<DeleteImageCollectionCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string IMAGE_COLLECTIONS_KEY = "image_collections";

    public DeleteImageCollectionHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<bool> Handle(DeleteImageCollectionCommand request, CancellationToken cancellationToken)
    {
        var collections = await _daprClient.GetStateAsync<List<ImageCollection>>(
            STORE_NAME,
            IMAGE_COLLECTIONS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<ImageCollection>();

        int initialCount = collections.Count;
        collections.RemoveAll(c => c.Id == request.Id);

        if (collections.Count == initialCount)
        {
            return false;
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            IMAGE_COLLECTIONS_KEY,
            collections,
            cancellationToken: cancellationToken
        );

        return true;
    }
}