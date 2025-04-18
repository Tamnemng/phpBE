using MediatR;
using Dapr.Client;
using OMS.Core.Queries;

public class GetAllGiftQueryHandler : IRequestHandler<GetAllGiftQuery, PagedModel<Gift>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string GIFT_METADATA_KEY = "gifts";

    public GetAllGiftQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<PagedModel<Gift>> Handle(GetAllGiftQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<Gift>(0, new List<Gift>(), 0, request.PageSize);
        }
        
        var giftMetadataList = await _daprClient.GetStateAsync<List<GiftMetaData>>(
            STORE_NAME, 
            GIFT_METADATA_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<GiftMetaData>();

        var totalCount = giftMetadataList.Count;
        if (totalCount == 0)
        {
            return new PagedModel<Gift>(0, new List<Gift>(), request.PageIndex, request.PageSize);
        }

        // Map GiftMetaData to Gift
        var gifts = giftMetadataList.Select(gm => new Gift(gm.Name, gm.Code, gm.Image) { Id = gm.Id }).ToList();

        // Pagination
        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<Gift>(totalCount, new List<Gift>(), request.PageIndex, request.PageSize);
        }

        var pagedGifts = gifts.Skip(startIndex).Take(request.PageSize).ToList();

        return new PagedModel<Gift>(totalCount, pagedGifts, request.PageIndex, request.PageSize);
    }
}