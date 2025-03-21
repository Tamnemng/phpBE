using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCT_LIST_KEY = "product_list";


    public AddProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, PRODUCT_LIST_KEY, cancellationToken: cancellationToken) ?? new List<string>();
        if (products.Contains(request.Id))
        {
            throw new System.Exception($"Product với ID '{request.Id}' đã tồn tại!");
        }

        products.Add(request.Id);
        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCT_LIST_KEY, products, cancellationToken: cancellationToken);

        await _daprClient.SaveStateAsync(STORE_NAME, request.Id, request, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}