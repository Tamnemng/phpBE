using MediatR;
using System.Collections.Generic;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class CategoryNameCodeDto
{
    public string Code { get; set; }
    public string Name { get; set; }

    public CategoryNameCodeDto(string code, string name)
    {
        Code = code;
        Name = name;
    }
}
public class GetAllCategoryNamesQuery : IRequest<List<CategoryNameCodeDto>>
{
    // No parameters needed as we're returning all gift names and codes
}


public class GetAllCategoryNamesQueryHandler : IRequestHandler<GetAllCategoryNamesQuery, List<CategoryNameCodeDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string GIFT_METADATA_KEY = "brands";

    public GetAllCategoryNamesQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<CategoryNameCodeDto>> Handle(GetAllCategoryNamesQuery request, CancellationToken cancellationToken)
    {
        var giftMetadataList = await _daprClient.GetStateAsync<List<CategoryMetaData>>(
            STORE_NAME, 
            GIFT_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<CategoryMetaData>();

        // Only map the Name and Code properties to the DTO
        var giftNamesAndCodes = giftMetadataList
            .Select(gm => new CategoryNameCodeDto(gm.Code, gm.Name))
            .ToList();

        return giftNamesAndCodes;
    }
}