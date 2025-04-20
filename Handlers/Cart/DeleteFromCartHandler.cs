using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class DeleteFromCartHandler : IRequestHandler<DeleteFromCartCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";

    public DeleteFromCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(DeleteFromCartCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        if (command.Items == null || !command.Items.Any())
        {
            throw new InvalidOperationException("No items specified for deletion.");
        }
        
        var cartList = await _daprClient.GetStateAsync<List<Cart>>(
            STORE_NAME,
            CART_METADATA_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Cart>();

        var existingCart = cartList.FirstOrDefault(c => c.UserId == command.UserId);
        if (existingCart == null)
        {
            throw new InvalidOperationException("Giỏ hàng không tồn tại!");
        }

        foreach (var item in command.Items)
        {
            existingCart.RemoveItem(item.ItemId, item.ItemType);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            CART_METADATA_KEY,
            cartList,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}