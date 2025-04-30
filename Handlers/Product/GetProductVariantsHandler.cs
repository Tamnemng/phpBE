using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetProductVariantsHandler : IRequestHandler<GetProductVariantsQuery, ProductVariantsDto>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public GetProductVariantsHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<ProductVariantsDto> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        // Get all products with the same code
        var productVariants = products.Where(p => p.ProductInfo.Code == request.ProductCode).ToList();
        
        if (!productVariants.Any())
        {
            throw new InvalidOperationException($"Product with code '{request.ProductCode}' not found.");
        }

        // Create the response DTO
        var result = new ProductVariantsDto
        {
            ProductCode = request.ProductCode,
            ProductName = productVariants.First().ProductInfo.Name,
            MainImageUrl = productVariants.First().ProductInfo.ImageUrl
        };

        // Get all variant groups
        var variantGroups = new Dictionary<string, HashSet<string>>();
        
        foreach (var product in productVariants)
        {
            foreach (var optionGroup in product.ProductOptions)
            {
                if (!variantGroups.ContainsKey(optionGroup.Title))
                {
                    variantGroups[optionGroup.Title] = new HashSet<string>();
                }
                
                foreach (var option in optionGroup.Options)
                {
                    variantGroups[optionGroup.Title].Add(option.Label);
                }
            }
        }

        // Add variant groups to the response
        foreach (var group in variantGroups)
        {
            result.VariantGroups.Add(new VariantGroupSummaryDto
            {
                OptionTitle = group.Key,
                Options = group.Value.ToList()
            });
        }

        // Add all product variants to the response
        foreach (var product in productVariants)
        {
            var variantDetail = new ProductVariantDetailDto
            {
                Id = product.ProductInfo.Id,
                Price = product.Price.CurrentPrice,
                OriginalPrice = product.Price.OriginalPrice,
                Quantity = product.ProductOptions.SelectMany(o => o.Options)
                                     .Count(o => o.Selected)
            };

            // Collect all selected options
            foreach (var optionGroup in product.ProductOptions)
            {
                var selectedOption = optionGroup.Options.FirstOrDefault(o => o.Selected);
                if (selectedOption != null)
                {
                    variantDetail.SelectedOptions[optionGroup.Title] = selectedOption.Label;
                }
            }

            // Add image URLs
            if (product.ProductDetail.Image != null)
            {
                variantDetail.ImageUrls = product.ProductDetail.Image
                    .OrderBy(i => i.piority)
                    .Select(i => i.Url)
                    .ToList();
            }

            result.Combinations.Add(variantDetail);
        }

        return result;
    }
}