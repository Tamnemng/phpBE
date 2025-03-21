using MediatR;
using Dapr.Client;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public UpdateProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingLaptop = await _daprClient.GetStateAsync<Dictionary<string, object>>(STORE_NAME, request.Id, cancellationToken: cancellationToken);

        if (existingLaptop == null)
        {
            throw new KeyNotFoundException($"Product với ID '{request.Id}' không tồn tại.");
        }

        var updatedLaptop = new Dictionary<string, object>
        {
            { "Id", request.Id },
            { "Name", request.Name },
            { "Labels", request.Labels },
        };

        await _daprClient.SaveStateAsync(STORE_NAME, request.Id, updatedLaptop, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
