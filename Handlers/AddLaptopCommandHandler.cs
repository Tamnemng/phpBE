using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
public class AddLaptopCommandHandler : IRequestHandler<AddLaptopCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string LAPTOP_LIST_KEY = "laptop_list";

    public AddLaptopCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddLaptopCommand request, CancellationToken cancellationToken)
    {
        var laptops = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, LAPTOP_LIST_KEY, cancellationToken: cancellationToken) ?? new List<string>();
        if (laptops.Contains(request.Id))
        {
            throw new System.Exception($"Laptop với ID '{request.Id}' đã tồn tại!");
        }

        laptops.Add(request.Id);
        await _daprClient.SaveStateAsync(STORE_NAME, LAPTOP_LIST_KEY, laptops, cancellationToken: cancellationToken);

        await _daprClient.SaveStateAsync(STORE_NAME, request.Id, request, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}