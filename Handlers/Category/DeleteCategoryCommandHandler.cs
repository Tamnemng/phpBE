
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
        var category = categories.FirstOrDefault(p => request.Id.Contains(p.Id));
        if (category == null)
        {
            return false;
        }
        
        categories.Remove(category);
        await _daprClient.SaveStateAsync(STORE_NAME, KEY, categories, cancellationToken: cancellationToken);
        return true;
    }
}