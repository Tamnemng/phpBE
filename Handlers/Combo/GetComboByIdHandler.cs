using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class GetComboByIdQueryHandler : IRequestHandler<GetComboByIdQuery, ComboDto>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string COMBOS_KEY = "combos";
    private const string PRODUCTS_KEY = "products";

    public GetComboByIdQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<ComboDto> Handle(GetComboByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        // Get all combos
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        var combo = combos.FirstOrDefault(c => c.Id == request.Id);
        
        if (combo == null)
        {
            throw new InvalidOperationException($"Combo with ID '{request.Id}' not found.");
        }
        
        // Get all products to include in response
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        var productList = new List<ProductSummaryDto>();
        
        foreach (var code in combo.ProductCodes)
        {
            var product = products.FirstOrDefault(p => p.ProductInfo.Code == code);
            
            if (product != null)
            {
                productList.Add(new ProductSummaryDto
                {
                    Id = product.ProductInfo.Id,
                    Code = product.ProductInfo.Code,
                    Name = product.ProductInfo.Name,
                    ImageUrl = product.ProductInfo.ImageUrl,
                    CurrentPrice = product.Price.CurrentPrice,
                    OriginalPrice = product.Price.OriginalPrice,
                    ShortDescription = product.ProductDetail.ShortDescription
                });
            }
        }
        
        return new ComboDto(combo, productList);
    }
}