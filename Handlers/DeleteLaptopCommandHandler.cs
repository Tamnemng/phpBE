
using MediatR;
using Dapr.Client;

public class DeleteLaptopCommandHandler : IRequestHandler<DeleteLaptopCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string LAPTOP_LIST_KEY = "laptop_list";

    public DeleteLaptopCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(DeleteLaptopCommand request, CancellationToken cancellationToken)
    {
        var laptops = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, LAPTOP_LIST_KEY, cancellationToken: cancellationToken) ?? new List<string>();
        if (laptops.Contains(request.Id))
        {
            laptops.Remove(request.Id);
            await _daprClient.SaveStateAsync(STORE_NAME, LAPTOP_LIST_KEY, laptops, cancellationToken: cancellationToken);
        }
        await _daprClient.DeleteStateAsync(STORE_NAME, request.Id, cancellationToken: cancellationToken);

        return Unit.Value;
    }

}