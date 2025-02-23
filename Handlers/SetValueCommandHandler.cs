using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class SetValueCommandHandler : IRequestHandler<SetValueCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY_LIST = "key_list";

    public SetValueCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(SetValueCommand request, CancellationToken cancellationToken)
    {
        var keys = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, KEY_LIST, cancellationToken: cancellationToken) ?? new List<string>();
        if (keys.Contains(request.Key))
        {
            throw new System.Exception($"Key '{request.Key}' đã tồn tại. Không thể tạo mới.");
        }
        keys.Add(request.Key);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY_LIST, keys, cancellationToken: cancellationToken);
        await _daprClient.SaveStateAsync(STORE_NAME, request.Key, request.Value, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
