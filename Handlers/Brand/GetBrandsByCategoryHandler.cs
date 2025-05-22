using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetBrandsByCategoryHandler : IRequestHandler<GetBrandsByCategoryQuery, List<BrandNameCodeDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";

    public GetBrandsByCategoryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<BrandNameCodeDto>> Handle(GetBrandsByCategoryQuery request, CancellationToken cancellationToken)
    {
        // Get all products
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        // Filter products by category
        var productsInCategory = products.Where(p => 
            p.ProductInfo.Category != null && 
            p.ProductInfo.Category.Contains(request.CategoryCode)
        ).ToList();

        // Get unique brand codes from filtered products
        var brandCodes = productsInCategory
            .Select(p => p.ProductInfo.Brand)
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .ToList();

        if (!brandCodes.Any())
        {
            return new List<BrandNameCodeDto>();
        }

        // Get brand metadata
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME,
            BRANDS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        // Filter and map brand metadata to DTOs
        var brandsInCategory = brands
            .Where(b => brandCodes.Contains(b.Code))
            .Select(b => new BrandNameCodeDto(b.Code, b.Name, b.Image))
            .ToList();

        return brandsInCategory;
    }
}