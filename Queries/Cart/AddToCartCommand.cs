using MediatR;
using System.Text.Json.Serialization;

public class AddToCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public string ItemId { get; set; } // ProductId hoặc ComboId
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemType ItemType { get; set; }
    public int Quantity { get; set; }

    public AddToCartCommand(string userId, string itemId, CartItemType itemType, int quantity)
    {
        UserId = userId;
        ItemId = itemId;
        ItemType = itemType;
        Quantity = quantity;
    }
}