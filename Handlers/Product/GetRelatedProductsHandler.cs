// Handlers/Product/GetRelatedProductsHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetRelatedProductsHandler : IRequestHandler<GetRelatedProductsQuery, List<ProductSummaryDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";

    public GetRelatedProductsHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<ProductSummaryDto>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken)
    {
        // Get all products
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        // Find the source product
        var sourceProduct = products.FirstOrDefault(p => p.ProductInfo.Code == request.ProductCode);
        
        if (sourceProduct == null)
        {
            throw new InvalidOperationException($"Product with code '{request.ProductCode}' not found.");
        }

        // Get brands for display names
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME,
            BRANDS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        var brandDict = brands.ToDictionary(b => b.Code, b => b.Name);

        // Get unique products (not variants)
        var uniqueProducts = products
            .GroupBy(p => p.ProductInfo.Code)
            .Select(g => g.First())
            .Where(p => p.ProductInfo.Code != request.ProductCode) // Exclude source product
            .ToList();

        // Calculate similarity scores for each product
        var scoredProducts = uniqueProducts.Select(p => new
        {
            Product = p,
            Score = CalculateSimilarityScore(sourceProduct, p)
        })
        .OrderByDescending(x => x.Score)
        .Take(request.Count)
        .ToList();

        // Convert to DTOs
        var relatedProducts = scoredProducts.Select(x => 
        {
            string brandName = string.Empty;
            if (!string.IsNullOrEmpty(x.Product.ProductInfo.Brand) && brandDict.ContainsKey(x.Product.ProductInfo.Brand))
            {
                brandName = brandDict[x.Product.ProductInfo.Brand];
            }

            return new ProductSummaryDto(x.Product, brandName);
        }).ToList();

        return relatedProducts;
    }

    private double CalculateSimilarityScore(Product sourceProduct, Product targetProduct)
    {
        double score = 0;

        // 1. Same brand is a strong signal (weight: 3)
        if (sourceProduct.ProductInfo.Brand == targetProduct.ProductInfo.Brand)
        {
            score += 3;
        }

        // 2. Category overlap (weight: 2 per matching category)
        if (sourceProduct.ProductInfo.Category != null && targetProduct.ProductInfo.Category != null)
        {
            var sourceCategories = sourceProduct.ProductInfo.Category.ToList();
            var targetCategories = targetProduct.ProductInfo.Category.ToList();

            int matchingCategories = sourceCategories.Intersect(targetCategories).Count();
            score += matchingCategories * 2;
        }

        // 3. Price similarity (weight: up to 3)
        // The closer the price, the higher the score
        var sourcePriceRange = GetPriceRange(sourceProduct.Price.CurrentPrice);
        var targetPriceRange = GetPriceRange(targetProduct.Price.CurrentPrice);

        // If prices are in the exact same range
        if (sourcePriceRange == targetPriceRange)
        {
            score += 3;
        }
        // If prices are in adjacent ranges
        else if (Math.Abs(sourcePriceRange - targetPriceRange) == 1)
        {
            score += 1.5;
        }

        // 4. Price percentage difference (weight: up to 2)
        // Calculate percentage difference between prices
        var priceDiff = Math.Abs(sourceProduct.Price.CurrentPrice - targetProduct.Price.CurrentPrice);
        var avgPrice = (sourceProduct.Price.CurrentPrice + targetProduct.Price.CurrentPrice) / 2;
        var percentageDiff = (double)(priceDiff / avgPrice);

        // Score inversely proportional to percentage difference (max 2 points)
        if (percentageDiff < 0.1) // Less than 10% difference
            score += 2;
        else if (percentageDiff < 0.2) // Less than 20% difference
            score += 1.5;
        else if (percentageDiff < 0.3) // Less than 30% difference
            score += 1;
        else if (percentageDiff < 0.5) // Less than 50% difference
            score += 0.5;

        // 5. Gift similarities (weight: 1)
        if (sourceProduct.Gifts != null && targetProduct.Gifts != null &&
            sourceProduct.Gifts.Any() && targetProduct.Gifts.Any())
        {
            var sourceGiftCodes = sourceProduct.Gifts.Select(g => g.Code).ToList();
            var targetGiftCodes = targetProduct.Gifts.Select(g => g.Code).ToList();

            int matchingGifts = sourceGiftCodes.Intersect(targetGiftCodes).Count();
            score += matchingGifts * 1;
        }

        return score;
    }

    // Helper method to determine price range
    private int GetPriceRange(decimal price)
    {
        if (price < 500000) return 1; // Under 500k
        if (price < 1000000) return 2; // 500k-1M
        if (price < 2000000) return 3; // 1M-2M
        if (price < 5000000) return 4; // 2M-5M
        if (price < 10000000) return 5; // 5M-10M
        return 6; // Over 10M
    }
}