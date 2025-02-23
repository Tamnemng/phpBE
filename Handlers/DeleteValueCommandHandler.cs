using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

public class DeleteValueCommandHandler : IRequestHandler<DeleteValueCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY_LIST = "key_list";

    public DeleteValueCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(DeleteValueCommand request, CancellationToken cancellationToken)
    {
        var keys = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, KEY_LIST, cancellationToken: cancellationToken) ?? new List<string>();
        if (keys.Contains(request.Key))
        {
            keys.Remove(request.Key);
            await _daprClient.SaveStateAsync(STORE_NAME, KEY_LIST, keys, cancellationToken: cancellationToken);
        }
        await _daprClient.DeleteStateAsync(STORE_NAME, request.Key, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
