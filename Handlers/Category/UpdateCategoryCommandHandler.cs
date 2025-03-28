using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "categories";

    public UpdateCategoryCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        var categories = await _daprClient.GetStateAsync<List<CategoryMetaData>>(STORE_NAME, KEY, cancellationToken: cancellationToken)
            ?? new List<CategoryMetaData>();
            
        var existingCategory = categories.FirstOrDefault(p => p.Id == request.Id);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException($"Category với ID '{request.Id}' không tồn tại.");
        }
        existingCategory.Update(request);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, categories, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}