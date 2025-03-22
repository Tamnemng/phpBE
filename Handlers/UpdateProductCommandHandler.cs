using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CATEGORY_PREFIX = "category_";

    public UpdateProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
    
        var existingProduct = await _daprClient.GetStateAsync<Dictionary<string, object>>(STORE_NAME, request.Id, cancellationToken: cancellationToken);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product với ID '{request.Id}' không tồn tại.");
        }

        string oldCategoryId = existingProduct.ContainsKey("CategoryId") ? existingProduct["CategoryId"].ToString() : null;

        if (!string.IsNullOrEmpty(oldCategoryId) && oldCategoryId != request.CategoryId)
        {
            var oldCategoryKey = $"{CATEGORY_PREFIX}{oldCategoryId}";
            var oldCategoryProducts = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, oldCategoryKey, cancellationToken: cancellationToken) ?? new List<string>();

            if (oldCategoryProducts.Contains(request.Id))
            {
                oldCategoryProducts.Remove(request.Id);
                await _daprClient.SaveStateAsync(STORE_NAME, oldCategoryKey, oldCategoryProducts, cancellationToken: cancellationToken);
            }
        }

        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var newCategoryKey = $"{CATEGORY_PREFIX}{request.CategoryId}";
            var newCategoryProducts = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, newCategoryKey, cancellationToken: cancellationToken) ?? new List<string>();

            if (!newCategoryProducts.Contains(request.Id)) 
            {
                newCategoryProducts.Add(request.Id);
                await _daprClient.SaveStateAsync(STORE_NAME, newCategoryKey, newCategoryProducts, cancellationToken: cancellationToken);
            }
        }

        var updatedProduct = new Dictionary<string, object>
        {
            { "Id", request.Id },
            { "Name", request.Name },
            { "CategoryId", request.CategoryId },
            { "Labels", request.Labels }
        };

        await _daprClient.SaveStateAsync(STORE_NAME, request.Id, updatedProduct, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
