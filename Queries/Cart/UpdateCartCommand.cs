using MediatR;
using System.Text.Json.Serialization;

public class UpdateCartCommand : IRequest<Unit>
{
    public string UserId { get; set; }
    public string OldItemId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemType OldItemType { get; set; }
    public string NewItemId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemType NewItemType { get; set; }
    public int Quantity { get; set; }

    public UpdateCartCommand(string userId, string oldItemId, CartItemType oldItemType, 
                            string newItemId, CartItemType newItemType, int quantity)
    {
        UserId = userId;
        OldItemId = oldItemId;
        OldItemType = oldItemType;
        NewItemId = newItemId;
        NewItemType = newItemType;
        Quantity = quantity;
    }
}