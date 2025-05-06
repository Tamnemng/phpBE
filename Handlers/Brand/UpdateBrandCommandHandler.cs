using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "brands";

    public UpdateBrandCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateBrandCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        var brandMetadataList = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME, 
            KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();
        
        var brandToUpdate = brandMetadataList.FirstOrDefault(b => 
            string.Equals(b.Id, command.Id, StringComparison.OrdinalIgnoreCase));

        if (brandToUpdate == null)
        {
            throw new InvalidOperationException(
                $"Brand with id '{command.Id}' not found."
            );
        }

        // If image is null, keep the existing image
        if (command.Image == null)
        {
            // Don't update the image, only update other properties
            brandToUpdate.UpdateWithoutImage(command, command.UpdatedBy);
        }
        else
        {
            // Update all properties including the image
            brandToUpdate.Update(command, command.UpdatedBy);
        }
        
        await _daprClient.SaveStateAsync(
            STORE_NAME, 
            KEY, 
            brandMetadataList, 
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}