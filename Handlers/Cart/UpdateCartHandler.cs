using MediatR;
using Dapr.Client;

public class UpdateCartHandler : IRequestHandler<UpdateCartCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";

    public UpdateCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateCartCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
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

        if (command.OldProductId != command.NewProductId)
        {
            existingCart.RemoveItem(command.OldProductId);

            var newItem = existingCart.Items.FirstOrDefault(i => i.ProductId == command.NewProductId);
            if (newItem != null)
            {
                existingCart.UpdateItemQuantity(command.NewProductId, newItem.Quantity + command.Quantity);
            }
            else
            {
                existingCart.AddItem(new CartItem(command.NewProductId, command.Quantity));
            }
        }
        else
        {
            existingCart.UpdateItemQuantity(command.NewProductId, command.Quantity);
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