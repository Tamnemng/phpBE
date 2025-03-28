using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System.Text.Json;

public class GetAllCategoryQueryHandler : IRequestHandler<GetAllCategoryQuery, PagedModel<Category>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string KEY = "categories";

    public GetAllCategoryQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PagedModel<Category>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<Category>(0, new List<Category>(), 0, request.PageSize);
        }
        var categories = await _daprClient.GetStateAsync<List<Category>>(STORE_NAME, KEY, cancellationToken: cancellationToken) 
            ?? new List<Category>();

        var totalCount = categories.Count;
        if (totalCount == 0)
        {
            return new PagedModel<Category>(0, new List<Category>(), request.PageIndex, request.PageSize);
        }

        // Phân trang
        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<Category>(totalCount, new List<Category>(), request.PageIndex, request.PageSize);
        }

        var pagedCategories = categories.Skip(startIndex).Take(request.PageSize).ToList();

        return new PagedModel<Category>(totalCount, pagedCategories, request.PageIndex, request.PageSize);
    }
}