using MediatR;
using OMS.Core.Queries;

public class GetCartQuery : IRequest<CartResponseDto> {
    public string userId { get; set; }
    public GetCartQuery(string userId)
    {
        this.userId = userId;
    }
}

public class GetItemsDetailsQuery : IRequest<List<ItemDetailsResponseDto>> 
{
    public List<ItemRequest> Items { get; set; }
    
    public GetItemsDetailsQuery(List<ItemRequest> items)
    {
        Items = items ?? new List<ItemRequest>();
    }
}

public class ItemDetailsResponseDto
{
    public string ItemId { get; set; }
    public CartItemType ItemType { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public List<ProductSummaryDto> ComboProducts { get; set; } = new List<ProductSummaryDto>();
}