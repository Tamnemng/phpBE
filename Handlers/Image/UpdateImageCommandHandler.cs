using MediatR;
using Dapr.Client;
public class UpdateImageCollectionHandler : IRequestHandler<UpdateImageCollectionCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string IMAGE_COLLECTIONS_KEY = "image_collections";

    public UpdateImageCollectionHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateImageCollectionCommand request, CancellationToken cancellationToken)
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

        // Update title if provided
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            collection.Title = request.Title;
        }

        // Remove deleted images
        if (request.DeletedImageUrls != null && request.DeletedImageUrls.Any())
        {
            collection.Images.RemoveAll(img => request.DeletedImageUrls.Contains(img.Url));
        }

        // Update existing images
        if (request.UpdatedImages != null)
        {
            foreach (var updatedImage in request.UpdatedImages)
            {
                var existingImage = collection.Images.FirstOrDefault(img => img.Url == updatedImage.Url);
                if (existingImage != null)
                {
                    existingImage.Priority = updatedImage.Priority;
                }
            }
        }

        // Add new images
        if (request.NewImages != null && request.NewImages.Any())
        {
            collection.Images.AddRange(request.NewImages);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            IMAGE_COLLECTIONS_KEY,
            collections,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}