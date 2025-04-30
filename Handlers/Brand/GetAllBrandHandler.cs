using MediatR;
using System.Collections.Generic;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BrandNameCodeDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; } // Optional, if you want to include image URL

    public BrandNameCodeDto(string code, string name, string imageUrl = null)
    {
        Code = code;
        Name = name;
        ImageUrl = imageUrl;
    }
}
public class GetAllBrandNamesQuery : IRequest<List<BrandNameCodeDto>>
{
    // No parameters needed as we're returning all gift names and codes
}


public class GetAllBrandNamesQueryHandler : IRequestHandler<GetAllBrandNamesQuery, List<BrandNameCodeDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string GIFT_METADATA_KEY = "brands";

    public GetAllBrandNamesQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<BrandNameCodeDto>> Handle(GetAllBrandNamesQuery request, CancellationToken cancellationToken)
    {
        var giftMetadataList = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME, 
            GIFT_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        // Only map the Name and Code properties to the DTO
        var giftNamesAndCodes = giftMetadataList
            .Select(gm => new BrandNameCodeDto(gm.Code, gm.Name, gm.Image))
            .ToList();

        return giftNamesAndCodes;
    }
}