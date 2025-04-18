using MediatR;
using System.Collections.Generic;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GiftNameCodeDto
{
    public string Code { get; set; }
    public string Name { get; set; }

    public GiftNameCodeDto(string code, string name)
    {
        Code = code;
        Name = name;
    }
}
public class GetAllGiftNamesQuery : IRequest<List<GiftNameCodeDto>>
{
    // No parameters needed as we're returning all gift names and codes
}


public class GetAllGiftNamesQueryHandler : IRequestHandler<GetAllGiftNamesQuery, List<GiftNameCodeDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string GIFT_METADATA_KEY = "gifts";

    public GetAllGiftNamesQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<GiftNameCodeDto>> Handle(GetAllGiftNamesQuery request, CancellationToken cancellationToken)
    {
        var giftMetadataList = await _daprClient.GetStateAsync<List<GiftMetaData>>(
            STORE_NAME, 
            GIFT_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<GiftMetaData>();

        // Only map the Name and Code properties to the DTO
        var giftNamesAndCodes = giftMetadataList
            .Select(gm => new GiftNameCodeDto(gm.Code, gm.Name))
            .ToList();

        return giftNamesAndCodes;
    }
}