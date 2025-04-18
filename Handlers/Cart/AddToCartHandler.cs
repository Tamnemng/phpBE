using MediatR;
using Dapr.Client;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";

    public AddToCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(AddToCartCommand command, CancellationToken cancellationToken)
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
        var newCartItem = new CartItem(command.ProductId, command.Quantity);
        existingCart.AddItem(newCartItem);

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            CART_METADATA_KEY,
            cartList,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}