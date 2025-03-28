using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AddBrandCommandHandler : IRequestHandler<AddBrandCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string BRAND_METADATA_KEY = "brands";

    public AddBrandCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(AddBrandCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        var brandMetadataList = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME, 
            BRAND_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        // Check for duplicate brand code
        if (brandMetadataList.Any(bm => 
            string.Equals(bm.Code, command.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"A brand with code '{command.Code}' already exists."
            );
        }

        // Create brand metadata directly using command details
        var brandMetadata = new BrandMetaData(
            createdBy: command.CreatedBy, 
            code: command.Code, 
            name: command.Name, 
            logo: command.Logo
        );

        // Add to collection
        brandMetadataList.Add(brandMetadata);

        // Save brand metadata
        await _daprClient.SaveStateAsync(
            STORE_NAME, 
            BRAND_METADATA_KEY, 
            brandMetadataList, 
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}