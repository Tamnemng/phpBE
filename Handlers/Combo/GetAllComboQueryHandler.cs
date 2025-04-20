using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Queries;

public class GetAllCombosQueryHandler : IRequestHandler<GetAllCombosQuery, PagedModel<ComboDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string COMBOS_KEY = "combos";
    private const string PRODUCTS_KEY = "products";

    public GetAllCombosQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<PagedModel<ComboDto>> Handle(GetAllCombosQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<ComboDto>(0, new List<ComboDto>(), 0, request.PageSize);
        }
        
        // Get all combos
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();
        
        // Filter inactive combos if required
        if (!request.IncludeInactive)
        {
            combos = combos.Where(c => c.IsActive).ToList();
        }
        
        var totalCount = combos.Count;
        
        if (totalCount == 0)
        {
            return new PagedModel<ComboDto>(0, new List<ComboDto>(), request.PageIndex, request.PageSize);
        }
        
        // Get all products to include in response
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        // Create product lookup by code
        var productDictionary = new Dictionary<string, ProductSummaryDto>();
        foreach (var product in products)
        {
            if (!productDictionary.ContainsKey(product.ProductInfo.Code))
            {
                productDictionary[product.ProductInfo.Code] = new ProductSummaryDto
                {
                    Id = product.ProductInfo.Id,
                    Code = product.ProductInfo.Code,
                    Name = product.ProductInfo.Name,
                    ImageUrl = product.ProductInfo.ImageUrl,
                    CurrentPrice = product.Price.CurrentPrice,
                    OriginalPrice = product.Price.OriginalPrice,
                    ShortDescription = product.ProductDetail.ShortDescription
                };
            }
        }
        
        // Pagination
        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<ComboDto>(totalCount, new List<ComboDto>(), request.PageIndex, request.PageSize);
        }
        
        var pagedCombos = combos
            .Skip(startIndex)
            .Take(request.PageSize)
            .ToList();
        
        // Map to DTOs with products
        var comboDtos = new List<ComboDto>();
        
        foreach (var combo in pagedCombos)
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
        
        return new PagedModel<ComboDto>(totalCount, comboDtos, request.PageIndex, request.PageSize);
    }
}