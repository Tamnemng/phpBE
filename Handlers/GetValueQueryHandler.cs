using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
public class GetValueQueryHandler : IRequestHandler<GetValueQuery, string>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public GetValueQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<string> Handle(GetValueQuery request, CancellationToken cancellationToken)
    {
        return await _daprClient.GetStateAsync<string>(STORE_NAME, request.Key, cancellationToken: cancellationToken);
    }
}