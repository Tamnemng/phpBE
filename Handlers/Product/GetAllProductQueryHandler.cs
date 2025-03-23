using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System.Text.Json;

public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, PagedModel<Product>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public GetAllProductQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PagedModel<Product>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<Product>(0, new List<Product>(), 0, request.PageSize);
        }
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken) 
            ?? new List<Product>();

        var totalCount = products.Count;
        if (totalCount == 0)
        {
            return new PagedModel<Product>(0, new List<Product>(), request.PageIndex, request.PageSize);
        }

        // Phân trang
        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<Product>(totalCount, new List<Product>(), request.PageIndex, request.PageSize);
        }

        var pagedProducts = products.Skip(startIndex).Take(request.PageSize).ToList();

        return new PagedModel<Product>(totalCount, pagedProducts, request.PageIndex, request.PageSize);
    }
}