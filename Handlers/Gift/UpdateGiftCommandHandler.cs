using MediatR;
using Dapr.Client;
public class UpdateGiftCommandHandler : IRequestHandler<UpdateGiftCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string GIFT_METADATA_KEY = "gifts";

    public UpdateGiftCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateGiftCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        var giftMetadataList = await _daprClient.GetStateAsync<List<GiftMetaData>>(
            STORE_NAME, 
            GIFT_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<GiftMetaData>();

        var giftToUpdate = giftMetadataList.FirstOrDefault(g => 
            string.Equals(g.Id, command.Id, StringComparison.OrdinalIgnoreCase));

        if (giftToUpdate == null)
        {
            throw new InvalidOperationException(
                $"A gift with id '{command.Id}' does not exist."
            );
        }

        giftToUpdate.Update(command, command.UpdatedBy);

        await _daprClient.SaveStateAsync(
            STORE_NAME, 
            GIFT_METADATA_KEY, 
            giftMetadataList, 
            cancellationToken: cancellationToken
        );
        
        return Unit.Value;
    }
}