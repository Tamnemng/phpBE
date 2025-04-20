using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class GetCombosByProductCodeQueryHandler : IRequestHandler<GetCombosByProductCodeQuery, List<ComboDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string COMBOS_KEY = "combos";
    private const string PRODUCTS_KEY = "products";

    public GetCombosByProductCodeQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<ComboDto>> Handle(GetCombosByProductCodeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        if (string.IsNullOrEmpty(request.ProductCode))
        {
            throw new InvalidOperationException("Product code must be provided.");
        }
        
        // Verify product exists
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        var product = products.FirstOrDefault(p => p.ProductInfo.Code == request.ProductCode);
        
        if (product == null)
        {
            throw new InvalidOperationException($"Product with code '{request.ProductCode}' not found.");
        }
        
        // Get all combos
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        // Filter combos that contain the product code and are active
        var filteredCombos = combos
            .Where(c => c.IsActive && c.ProductCodes.Contains(request.ProductCode))
            .ToList();
        
        if (!filteredCombos.Any())
        {
            return new List<ComboDto>();
        }
        
        // Create product dictionary for lookup
        var productDictionary = new Dictionary<string, ProductSummaryDto>();
        foreach (var p in products)
        {
            if (!productDictionary.ContainsKey(p.ProductInfo.Code))
            {
                productDictionary[p.ProductInfo.Code] = new ProductSummaryDto
                {
                    Id = p.ProductInfo.Id,
                    Code = p.ProductInfo.Code,
                    Name = p.ProductInfo.Name,
                    ImageUrl = p.ProductInfo.ImageUrl,
                    CurrentPrice = p.Price.CurrentPrice,
                    OriginalPrice = p.Price.OriginalPrice,
                    ShortDescription = p.ProductDetail.ShortDescription
                };
            }
        }
        
        // Map to DTOs with products
        var comboDtos = new List<ComboDto>();
        
        foreach (var combo in filteredCombos)
        {
            var productList = new List<ProductSummaryDto>();
            
            foreach (var code in combo.ProductCodes)
            {
                if (productDictionary.TryGetValue(code, out var productDto))
                {
                    productList.Add(productDto);
                }
            }
            
            comboDtos.Add(new ComboDto(combo, productList));
        }
        
        return comboDtos;
    }
}