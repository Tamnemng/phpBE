using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

public class SetValueCommandHandler : IRequestHandler<SetValueCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public SetValueCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(SetValueCommand request, CancellationToken cancellationToken)
    {
        await _daprClient.SaveStateAsync(STORE_NAME, request.Key, request.Value, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}