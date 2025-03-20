using MediatR;
using Dapr.Client;

public class UpdateLaptopCommandHandler : IRequestHandler<UpdateLaptopCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string LAPTOP_LIST_KEY = "laptop_list";

    public UpdateLaptopCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateLaptopCommand request, CancellationToken cancellationToken)
    {
        var laptops = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, LAPTOP_LIST_KEY, cancellationToken: cancellationToken) ?? new List<string>();
        if (!laptops.Contains(request.Id))
        {
            throw new System.Exception($"Id `{request.Id}` không tồn tại. Không thể cập nhật.");
        }

        await _daprClient.SaveStateAsync(STORE_NAME, request.Id, request.NewValue, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}