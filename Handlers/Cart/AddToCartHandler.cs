using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";
    private const string PRODUCTS_KEY = "products";
    private const string COMBOS_KEY = "combos";

    public AddToCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(AddToCartCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Verify the item exists based on type
        if (command.ItemType == CartItemType.Product)
        {
            await VerifyProductExists(command.ItemId, cancellationToken);
        }
        else if (command.ItemType == CartItemType.Combo)
        {
            await VerifyComboExists(command.ItemId, cancellationToken);
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

        var existingItem = existingCart.Items.FirstOrDefault(i => 
            i.ItemId == command.ItemId && i.ItemType == command.ItemType);
            
        if (existingItem != null)
        {
            existingCart.UpdateItemQuantity(command.ItemId, command.ItemType, existingItem.Quantity + command.Quantity);
        }
        else
        {
            existingCart.AddItem(new CartItem(command.ItemId, command.ItemType, command.Quantity));
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