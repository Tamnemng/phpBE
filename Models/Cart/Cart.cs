using OMS.Core.Utilities;
using System.Collections.Generic;

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

    public void RemoveItem(string productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
    }

    public void UpdateItemQuantity(string productId, int quantity)
    {
        var item = Items.Find(i => i.ProductId == productId);
        if (item != null && quantity > 0)
        {
            item.Quantity = quantity;
        }
    }
}

public class CartItem
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public CartItem(string productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}