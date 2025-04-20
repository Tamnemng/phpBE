using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class AddComboCommandHandler : IRequestHandler<AddComboCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string COMBOS_KEY = "combos";
    private const string PRODUCTS_KEY = "products";

    public AddComboCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(AddComboCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        if (command.ProductCodes == null || !command.ProductCodes.Any())
        {
            throw new InvalidOperationException("At least one product code must be provided for a combo.");
        }
        
        // Check if all product codes exist
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        var existingProductCodes = products.Select(p => p.ProductInfo.Code).ToList();
        
        var invalidProductCodes = command.ProductCodes
            .Where(code => !existingProductCodes.Contains(code))
            .ToList();
            
        if (invalidProductCodes.Any())
        {
            throw new InvalidOperationException(
                $"The following product codes do not exist: {string.Join(", ", invalidProductCodes)}"
            );
        }
        
        // Calculate original price by summing prices of all products
        decimal originalPrice = 0;
        foreach (var code in command.ProductCodes)
        {
            var product = products.FirstOrDefault(p => p.ProductInfo.Code == code);
            if (product != null)
            {
                originalPrice += product.Price.CurrentPrice;
            }
        }
        
        // Get existing combos
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        // Create new combo
        var combo = new Combo(
            command.Name,
            command.Description,
            command.ImageUrl,
            command.ProductCodes,
            command.ComboPrice,
            command.CreatedBy
        );
        
        combo.UpdateDiscountInfo(originalPrice);
        
        combos.Add(combo);
        
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            COMBOS_KEY,
            combos,
            cancellationToken: cancellationToken
        );
        
        return Unit.Value;
    }
}