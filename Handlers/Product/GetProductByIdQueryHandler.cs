
using Dapr.Client;
using MediatR;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Product>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public GetProductByIdQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken) 
            ?? new List<Product>();
        var product = products.FirstOrDefault(p => p.Id == request.Id);
        if (product == null)
        {
            throw new Exception($"Product với ID '{request.Id}' không tồn tại.");
        }

        return product;
    }
}