
using Dapr.Client;
using MediatR;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Dictionary<string, object>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public GetProductByIdQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Dictionary<string, object>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var data = await _daprClient.GetStateAsync<Dictionary<string, object>>(STORE_NAME, request.Id, cancellationToken: cancellationToken);
        return data ?? throw new Exception($"Product với ID '{request.Id}' không tồn tại.");
    }
}