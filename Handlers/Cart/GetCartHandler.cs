using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System.Text.Json;

public class GetCartHandler : IRequestHandler<GetCartQuery, Cart>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";

    public GetCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Cart> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var cartList = await _daprClient.GetStateAsync<List<Cart>>(
            STORE_NAME,
            CART_METADATA_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Cart>();

        var existingCart = cartList.FirstOrDefault(c => c.UserId == request.userId);
        Console.WriteLine(existingCart);
        return existingCart ?? throw new Exception("Cart not found");
    }
}