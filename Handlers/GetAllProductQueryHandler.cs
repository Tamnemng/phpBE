using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System.Text.Json;

public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, PagedModel<KeyValuePair<string, string>>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCT_LIST_KEY = "product_list";
    

    public GetAllProductQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PagedModel<KeyValuePair<string, string>>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<KeyValuePair<string, string>>(0, new List<KeyValuePair<string, string>>(), 0, request.PageSize);
        }

        var products = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, PRODUCT_LIST_KEY, cancellationToken: cancellationToken) ?? new List<string>();
        var totalCount = products.Count;
        if (totalCount == 0)
        {
            return new PagedModel<KeyValuePair<string, string>>(0, new List<KeyValuePair<string, string>>(), request.PageIndex, request.PageSize);
        }

        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<KeyValuePair<string, string>>(totalCount, new List<KeyValuePair<string, string>>(), request.PageIndex, request.PageSize);
        }

        var pagedProducts = products.Skip(startIndex).Take(request.PageSize).ToList();

        var values = new List<KeyValuePair<string, string>>();
        foreach (var key in pagedProducts)
        {
            var rawValue = await _daprClient.GetStateAsync<string>(STORE_NAME, key, cancellationToken: cancellationToken);
            var cleanedValue = !string.IsNullOrEmpty(rawValue) && rawValue.StartsWith("\"") && rawValue.EndsWith("\"")
                ? JsonSerializer.Deserialize<string>(rawValue)
                : rawValue;

            values.Add(new KeyValuePair<string, string>(key, cleanedValue ?? string.Empty));
        }

        return new PagedModel<KeyValuePair<string, string>>(totalCount, values, request.PageIndex, request.PageSize);
    }
}