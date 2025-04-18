using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Utilities;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";
    private const string CATEGORIES_KEY = "categories";
    private const string GIFTS_KEY = "gifts";

    public AddProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken)
            ?? new List<Product>();
        
        if (products.Any(p => p.ProductInfo.Code.Equals(command.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Product with code '{command.Code}' already exists.");
        }
        // Checking brand
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME, 
            BRANDS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        if (!brands.Any(b => b.Code == command.BrandCode))
        {
            throw new InvalidOperationException($"Brand with code '{command.BrandCode}' does not exist.");
        }

        // Checking categories
        var categories = await _daprClient.GetStateAsync<List<CategoryMetaData>>(
            STORE_NAME, 
            CATEGORIES_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<CategoryMetaData>();

        foreach (var categoryCode in command.CategoriesCode)
        {
            if (!categories.Any(c => c.Code == categoryCode))
            {
                throw new InvalidOperationException($"Category with code '{categoryCode}' does not exist.");
            }
        }

        if (command.GiftCodes != null && command.GiftCodes.Any())
        {
            var gifts = await _daprClient.GetStateAsync<List<GiftMetaData>>(
                STORE_NAME, 
                GIFTS_KEY, 
                cancellationToken: cancellationToken
            ) ?? new List<GiftMetaData>();
            
            foreach (var giftCode in command.GiftCodes)
            {
                var giftMetadata = gifts.FirstOrDefault(g => g.Code.Equals(giftCode, StringComparison.OrdinalIgnoreCase));
                if (giftMetadata == null)
                {
                    throw new InvalidOperationException($"Gift with code '{giftCode}' does not exist.");
                }
                
                // Create a Gift object from the metadata and add it to the command's Gifts collection
                command.Gifts.Add(new Gift(giftMetadata.Name, giftMetadata.Code, giftMetadata.Image));
            }
        }

        if (command.Variants != null && command.Variants.Any())
        {
            var variantsByOption = command.Variants
                .GroupBy(v => v.OptionTitle)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            var firstOptionTitle = variantsByOption.Keys.FirstOrDefault();
            
            if (firstOptionTitle != null)
            {
                var optionIdMap = new Dictionary<string, string>();
                foreach (var variant in variantsByOption[firstOptionTitle])
                {
                    optionIdMap[variant.OptionLabel] = IdGenerator.GenerateId(20);
                }
                
                foreach (var variant in variantsByOption[firstOptionTitle])
                {
                    string productId = optionIdMap[variant.OptionLabel];
                    var productVariation = new Product(command, productId, variant);
                    var productOptions = new List<ProductOption>();
                    var mainOptions = new List<Option>();
                    foreach (var optionLabel in optionIdMap.Keys)
                    {
                        bool isSelected = optionLabel == variant.OptionLabel;
                        mainOptions.Add(new Option(
                            optionLabel,
                            optionIdMap[optionLabel],
                            variantsByOption[firstOptionTitle].FirstOrDefault(v => v.OptionLabel == optionLabel)?.Quantity ?? 0,
                            isSelected
                        ));
                    }
                    productOptions.Add(new ProductOption(firstOptionTitle, mainOptions));
                    foreach (var optionTitle in variantsByOption.Keys.Where(k => k != firstOptionTitle))
                    {
                        var options = new List<Option>();
                        foreach (var otherVariant in variantsByOption[optionTitle])
                        {
                            options.Add(new Option(
                                otherVariant.OptionLabel,
                                IdGenerator.GenerateId(20),
                                otherVariant.Quantity,
                                false
                            ));
                        }
                        productOptions.Add(new ProductOption(optionTitle, options));
                    }
                    
                    productVariation.ProductOptions = productOptions;
                    products.Add(productVariation);
                }
            }
        }
        else
        {
            throw new InvalidOperationException("Phải cung cấp ít nhất một biến thể sản phẩm.");
        }

        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}