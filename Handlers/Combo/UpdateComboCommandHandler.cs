using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class UpdateComboCommandHandler : IRequestHandler<UpdateComboCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string COMBOS_KEY = "combos";
    private const string PRODUCTS_KEY = "products";

    public UpdateComboCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateComboCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Get existing combos
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        var combo = combos.FirstOrDefault(c => c.Id == command.Id);
        
        if (combo == null)
        {
            throw new InvalidOperationException($"Combo with ID '{command.Id}' not found.");
        }
        
        // Check if product codes exist if provided
        if (command.ProductCodes != null && command.ProductCodes.Any())
        {
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
        }
        
        // Update combo
        combo.Update(command, command.UpdatedBy);
        
        // Recalculate original price and discount if product codes or combo price changed
        if ((command.ProductCodes != null && command.ProductCodes.Any()) || command.ComboPrice.HasValue)
        {
            var products = await _daprClient.GetStateAsync<List<Product>>(
                STORE_NAME,
                PRODUCTS_KEY,
                cancellationToken: cancellationToken
            ) ?? new List<Product>();
            
            decimal originalPrice = 0;
            foreach (var code in combo.ProductCodes)
            {
                var product = products.FirstOrDefault(p => p.ProductInfo.Code == code);
                if (product != null)
                {
                    originalPrice += product.Price.CurrentPrice;
                }
            }
            
            combo.UpdateDiscountInfo(originalPrice);
        }
        
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            COMBOS_KEY,
            combos,
            cancellationToken: cancellationToken
        );
        
        return Unit.Value;
    }
}