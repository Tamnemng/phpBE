using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AddGiftCommandHandler : IRequestHandler<AddGiftCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string BRAND_METADATA_KEY = "gifts";

    public AddGiftCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(AddGiftCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        var giftMetadataList = await _daprClient.GetStateAsync<List<GiftMetaData>>(
            STORE_NAME, 
            BRAND_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<GiftMetaData>();

        if (giftMetadataList.Any(bm => 
            string.Equals(bm.Code, command.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"A gift with code '{command.Code}' already exists."
            );
        }

        var giftMetadata = new GiftMetaData(
            createdBy: command.CreatedBy, 
            code: command.Code, 
            name: command.Name, 
            image: command.Image
        );
        giftMetadataList.Add(giftMetadata);
        await _daprClient.SaveStateAsync(
            STORE_NAME, 
            BRAND_METADATA_KEY, 
            giftMetadataList, 
            cancellationToken: cancellationToken
        );
        return Unit.Value;
    }
}