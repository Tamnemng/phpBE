using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

public class DeleteValueCommandHandler : IRequestHandler<DeleteValueCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";

    public DeleteValueCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(DeleteValueCommand request, CancellationToken cancellationToken)
    {
        await _daprClient.DeleteStateAsync(STORE_NAME, request.Key, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}