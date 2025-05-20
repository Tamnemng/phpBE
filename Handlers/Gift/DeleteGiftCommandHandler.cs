using MediatR;
using Dapr.Client;
public class DeleteGiftCommandHandler : IRequestHandler<DeleteGiftCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string GIFT_METADATA_KEY = "gifts";

    public DeleteGiftCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<bool> Handle(DeleteGiftCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));

        if (command.Id == null || !command.Id.Any())
        {
            return false;
        }

        var giftMetadataList = await _daprClient.GetStateAsync<List<GiftMetaData>>(
            STORE_NAME,
            GIFT_METADATA_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<GiftMetaData>();

        int initialCount = giftMetadataList.Count;
        giftMetadataList = giftMetadataList
            .Where(g => !command.Id.Contains(g.Code))
            .ToList();

        if (giftMetadataList.Count == initialCount)
        {
            throw new InvalidOperationException($"Gift not found with CODE: {string.Join(", ", command.Id)}");
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            GIFT_METADATA_KEY,
            giftMetadataList,
            cancellationToken: cancellationToken
        );

        return true;
    }
}