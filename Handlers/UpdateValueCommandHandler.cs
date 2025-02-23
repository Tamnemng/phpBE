using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UpdateValueCommandHandler : IRequestHandler<UpdateValueCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY_LIST = "key_list";

    public UpdateValueCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateValueCommand request, CancellationToken cancellationToken)
    {
        var keys = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, KEY_LIST, cancellationToken: cancellationToken) ?? new List<string>();
        if (!keys.Contains(request.Key))
        {
            throw new System.Exception($"Key '{request.Key}' không tồn tại. Không thể cập nhật.");
        }
        await _daprClient.SaveStateAsync(STORE_NAME, request.Key, request.NewValue, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
