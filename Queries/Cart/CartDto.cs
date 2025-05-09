using System.ComponentModel.DataAnnotations;
using MediatR;
using System.Text.Json.Serialization;

public class AddCartDto
{
    [Required]
    public string ItemId { get; set; } // ProductId hoặc ComboId

    [Required]
    public CartItemType ItemType { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}

public class UpdateCartDto
{

    [Required]
    public string OldItemId { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemType OldItemType { get; set; }

    [Required]
    public string NewItemId { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemType NewItemType { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}

public class DeleteCartDto
{
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CartItemInfo[] ItemInfo { get; set; }
}

public class ItemRequest
{
    public string ItemId { get; set; }
    public CartItemType ItemType { get; set; }
}