using MediatR;
using Dapr.Client;
using Think4.Services;
public class AddImageCollectionHandler : IRequestHandler<AddImageCollectionCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private readonly ICloudinaryService _cloudinaryService;
    private const string STORE_NAME = "statestore";
    private const string IMAGE_COLLECTIONS_KEY = "image_collections";

    public AddImageCollectionHandler(DaprClient daprClient, ICloudinaryService cloudinaryService)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }

    public async Task<Unit> Handle(AddImageCollectionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        var collections = await _daprClient.GetStateAsync<List<ImageCollection>>(
            STORE_NAME, 
            IMAGE_COLLECTIONS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<ImageCollection>();

        var collection = new ImageCollection
        {
            Title = command.Title,
            Images = command.Images
        };
        
        collections.Add(collection);
        
        await _daprClient.SaveStateAsync(
            STORE_NAME, 
            IMAGE_COLLECTIONS_KEY, 
            collections, 
            cancellationToken: cancellationToken
        );
        
        return Unit.Value;
    }
}