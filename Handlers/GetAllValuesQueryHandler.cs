using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OMS.Core.Queries;
using System.Text.Json;

public class GetAllValuesQueryHandler : IRequestHandler<GetAllValuesQuery, PagedModel<KeyValuePair<string, string>>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY_LIST = "key_list";

    public GetAllValuesQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PagedModel<KeyValuePair<string, string>>> Handle(GetAllValuesQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<KeyValuePair<string, string>>(0, new List<KeyValuePair<string, string>>(), 0, request.PageSize);
        }
        var keys = await _daprClient.GetStateAsync<List<string>>(STORE_NAME, KEY_LIST, cancellationToken: cancellationToken) ?? new List<string>();
        var totalCount = keys.Count;
        if (totalCount == 0)
        {
            return new PagedModel<KeyValuePair<string, string>>(0, new List<KeyValuePair<string, string>>(), request.PageIndex, request.PageSize);
        }

        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<KeyValuePair<string, string>>(totalCount, new List<KeyValuePair<string, string>>(), request.PageIndex, request.PageSize);
        }

        var pagedKeys = keys.Skip(startIndex).Take(request.PageSize).ToList();

        var values = new List<KeyValuePair<string, string>>();
        foreach (var key in pagedKeys)
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
