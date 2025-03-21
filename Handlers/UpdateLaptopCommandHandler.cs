using MediatR;
using Dapr.Client;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UpdateLaptopCommandHandler : IRequestHandler<UpdateLaptopCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public UpdateLaptopCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateLaptopCommand request, CancellationToken cancellationToken)
    {
        var existingLaptop = await _daprClient.GetStateAsync<Dictionary<string, object>>(STORE_NAME, request.Id, cancellationToken: cancellationToken);

        if (existingLaptop == null)
        {
            throw new KeyNotFoundException($"Laptop với ID '{request.Id}' không tồn tại.");
        }

        var updatedLaptop = new Dictionary<string, object>
        {
            { "Id", request.Id },
            { "Name", request.Name },
            { "Brand", request.Brand },
            { "CPU", request.CPU },
            { "RAM", request.RAM },
            { "GPU", request.GPU },
            { "Storage", request.Storage },
            { "ScreenSize", request.ScreenSize },
            { "Price", request.Price },
            { "Usage", request.Usage }
        };

        await _daprClient.SaveStateAsync(STORE_NAME, request.Id, updatedLaptop, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
