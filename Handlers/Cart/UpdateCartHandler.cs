using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class UpdateCartHandler : IRequestHandler<UpdateCartCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";
    private const string PRODUCTS_KEY = "products";
    private const string COMBOS_KEY = "combos";

    public UpdateCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateCartCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Verify the new item exists if it's different from the old one
        if (command.OldItemId != command.NewItemId || command.OldItemType != command.NewItemType)
        {
            if (command.NewItemType == CartItemType.Product)
            {
                await VerifyProductExists(command.NewItemId, cancellationToken);
            }
            else if (command.NewItemType == CartItemType.Combo)
            {
                await VerifyComboExists(command.NewItemId, cancellationToken);
            }
        }
        
        var cartList = await _daprClient.GetStateAsync<List<Cart>>(
            STORE_NAME,
            CART_METADATA_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Cart>();

        var existingCart = cartList.FirstOrDefault(c => c.UserId == command.UserId);
        if (existingCart == null)
        {
            existingCart = new Cart(command.UserId, new List<CartItem>());
            cartList.Add(existingCart);
        }

        // If either item ID or type changed, remove old item and add new one
        if (command.OldItemId != command.NewItemId || command.OldItemType != command.NewItemType)
        {
            existingCart.RemoveItem(command.OldItemId, command.OldItemType);

            var newItem = existingCart.Items.FirstOrDefault(i => 
                i.ItemId == command.NewItemId && i.ItemType == command.NewItemType);
                
            if (newItem != null)
            {
                existingCart.UpdateItemQuantity(command.NewItemId, command.NewItemType, newItem.Quantity + command.Quantity);
            }
            else
            {
                existingCart.AddItem(new CartItem(command.NewItemId, command.NewItemType, command.Quantity));
            }
        }
        else
        {
            // Just update quantity
            existingCart.UpdateItemQuantity(command.NewItemId, command.NewItemType, command.Quantity);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            CART_METADATA_KEY,
            cartList,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
    
    private async Task VerifyProductExists(string productId, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        if (!products.Any(p => p.ProductInfo.Id == productId))
        {
            throw new InvalidOperationException($"Product with ID '{productId}' does not exist.");
        }
    }
    
    private async Task VerifyComboExists(string comboId, CancellationToken cancellationToken)
    {
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        if (!combos.Any(c => c.Id == comboId))
        {
            throw new InvalidOperationException($"Combo with ID '{comboId}' does not exist.");
        }
    }
}