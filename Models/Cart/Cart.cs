using OMS.Core.Utilities;
using System.Collections.Generic;

public enum CartItemType
{
    Product,
    Combo
}

public class Cart
{
    public string UserId { get; set; }
    public List<CartItem> Items { get; set; }

    public Cart(string userId, List<CartItem> items)
    {
        UserId = userId;
        Items = items ?? new List<CartItem>();
    }

    public void AddItem(CartItem item)
    {
        Items.Add(item);
    }

    public void RemoveItem(string itemId, CartItemType itemType)
    {
        Items.RemoveAll(i => i.ItemId == itemId && i.ItemType == itemType);
    }

    public void UpdateItemQuantity(string itemId, CartItemType itemType, int quantity)
    {
        var item = Items.Find(i => i.ItemId == itemId && i.ItemType == itemType);
        if (item != null && quantity > 0)
        {
            item.Quantity = quantity;
        }
    }
}

public class CartItem
{
    public string ItemId { get; set; } // Product ID or Combo ID
    public CartItemType ItemType { get; set; }
    public int Quantity { get; set; }
    
    public CartItem(string itemId, CartItemType itemType, int quantity)
    {
        ItemId = itemId;
        ItemType = itemType;
        Quantity = quantity;
    }
}