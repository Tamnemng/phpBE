using MediatR;
using System.Text.Json.Serialization;

public class DeleteFromCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public CartItemInfo[] Items { get; set; }

    public DeleteFromCartCommand(string userId, CartItemInfo[] items)
    {
        UserId = userId;
        Items = items;
    }
}

public class CartItemInfo
{
    public string ItemId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemType ItemType { get; set; }
    
    public CartItemInfo(string itemId, CartItemType itemType)
    {
        ItemId = itemId;
        ItemType = itemType;
    }
}