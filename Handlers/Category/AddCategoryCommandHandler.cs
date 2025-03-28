using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "categories";

    public AddCategoryCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddCategoryCommand command, CancellationToken cancellationToken)
    {
        var categories = await _daprClient.GetStateAsync<List<CategoryMetaData>>(STORE_NAME, KEY, cancellationToken: cancellationToken)
            ?? new List<CategoryMetaData>();
        if (categories.Any(b => b.Code.Equals(command.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A category with code '{command.Code}' already exists.");
        }
        var brandMetadata = new CategoryMetaData(
            command
        );
        categories.Add(brandMetadata);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, categories, cancellationToken: cancellationToken);
        return Unit.Value;
    }

}