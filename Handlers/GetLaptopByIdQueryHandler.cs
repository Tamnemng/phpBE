
using Dapr.Client;
using MediatR;

public class GetLaptopByIdQueryHandler : IRequestHandler<GetLaptopByIdQuery, Dictionary<string, object>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public GetLaptopByIdQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Dictionary<string, object>> Handle(GetLaptopByIdQuery request, CancellationToken cancellationToken)
    {
        var data = await _daprClient.GetStateAsync<Dictionary<string, object>>(STORE_NAME, request.Id, cancellationToken: cancellationToken);
        return data ?? throw new Exception($"Laptop với ID '{request.Id}' không tồn tại.");
    }
}