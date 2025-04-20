using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class DeleteComboCommandHandler : IRequestHandler<DeleteComboCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string COMBOS_KEY = "combos";

    public DeleteComboCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<bool> Handle(DeleteComboCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Get existing combos
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        int initialCount = combos.Count;
        
        combos.RemoveAll(c => c.Id == command.Id);
        
        if (combos.Count == initialCount)
        {
            // No combo was removed
            return false;
        }
        
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            COMBOS_KEY,
            combos,
            cancellationToken: cancellationToken
        );
        
        return true;
    }
}