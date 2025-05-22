using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedModel<ProductSummaryDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";

    public GetAllProductsQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PagedModel<ProductSummaryDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<ProductSummaryDto>(0, new List<ProductSummaryDto>(), 0, request.PageSize);
        }
        
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME,
            BRANDS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        var brandDict = brands.ToDictionary(b => b.Code, b => b.Name);

        var uniqueProducts = products
            .GroupBy(p => p.ProductInfo.Code)
            .Select(g => g.First())
            .ToList();
        
        var filteredProducts = uniqueProducts.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.ProductName))
        {
            filteredProducts = filteredProducts.Where(p => 
                p.ProductInfo.Name.Contains(request.ProductName, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(request.BrandCode))
        {
            filteredProducts = filteredProducts.Where(p => 
                p.ProductInfo.Brand == request.BrandCode);
        }
        
        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
        {
            filteredProducts = filteredProducts.Where(p => 
                p.ProductInfo.Category != null && 
                p.ProductInfo.Category.Contains(request.CategoryCode));
        }
        
        // Apply price range filter
        if (request.MinPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.Price.CurrentPrice >= request.MinPrice.Value);
        }
        
        if (request.MaxPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(p => p.Price.CurrentPrice <= request.MaxPrice.Value);
        }
        
        var filteredProductsList = filteredProducts.ToList();
        var productSummaries = filteredProductsList.Select(p => 
        {
            string brandName = string.Empty;
            if (!string.IsNullOrEmpty(p.ProductInfo.Brand) && brandDict.ContainsKey(p.ProductInfo.Brand))
            {
                brandName = brandDict[p.ProductInfo.Brand];
            }

            return new ProductSummaryDto(p, brandName);
        }).ToList();
        
        // Apply discount percentage filter
        if (request.MinDiscountPercentage.HasValue)
        {
            productSummaries = productSummaries
                .Where(p => p.DiscountPercentage >= request.MinDiscountPercentage.Value)
                .ToList();
        }
        
        // Apply sorting
        switch (request.SortBy)
        {
            case ProductSortOption.PriceAscending:
                productSummaries = productSummaries.OrderBy(p => p.CurrentPrice).ToList();
                break;
                
            case ProductSortOption.PriceDescending:
                productSummaries = productSummaries.OrderByDescending(p => p.CurrentPrice).ToList();
                break;
                
            case ProductSortOption.DiscountPercentageAscending:
                productSummaries = productSummaries.OrderBy(p => p.DiscountPercentage).ToList();
                break;
                
            case ProductSortOption.DiscountPercentageDescending:
                productSummaries = productSummaries.OrderByDescending(p => p.DiscountPercentage).ToList();
                break;
                
            case ProductSortOption.NameAscending:
                productSummaries = productSummaries.OrderBy(p => p.Name).ToList();
                break;
                
            case ProductSortOption.NameDescending:
                productSummaries = productSummaries.OrderByDescending(p => p.Name).ToList();
                break;
                
            case ProductSortOption.None:
            default:
                // No sorting applied
                break;
        }

        var totalCount = productSummaries.Count;
        if (totalCount == 0)
        {
            return new PagedModel<ProductSummaryDto>(0, new List<ProductSummaryDto>(), request.PageIndex, request.PageSize);
        }

        // Phân trang
        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<ProductSummaryDto>(totalCount, new List<ProductSummaryDto>(), request.PageIndex, request.PageSize);
        }

        var pagedProductSummaries = productSummaries.Skip(startIndex).Take(request.PageSize).ToList();

        return new PagedModel<ProductSummaryDto>(totalCount, pagedProductSummaries, request.PageIndex, request.PageSize);
    }
}