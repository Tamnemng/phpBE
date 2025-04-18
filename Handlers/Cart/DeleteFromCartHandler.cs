using MediatR;
using Dapr.Client;

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

        foreach (var productId in command.ProductIds)
        {
            existingCart.RemoveItem(productId);
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