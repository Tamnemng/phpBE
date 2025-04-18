
using MediatR;
using Dapr.Client;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "categories";

    public DeleteCategoryCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {

        var categories = await _daprClient.GetStateAsync<List<Category>>(STORE_NAME, KEY, cancellationToken: cancellationToken)
            ?? new List<Category>();

        var existingIds = categories.Select(b => b.Code).ToList();
        var nonExistingIds = request.Id.Where(id => !existingIds.Contains(id)).ToList();

        if (nonExistingIds.Any())
        {
            throw new InvalidOperationException($"Không tìm thấy tag với CODE: {string.Join(", ", nonExistingIds)}");
        }

        var initialCount = categories.Count;
        categories.RemoveAll(category => request.Id.Contains(category.Code));

        if (categories.Count == initialCount)
        {
            return false; // No brands were removed
        }
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, categories, cancellationToken: cancellationToken);
        return true;
    }
}