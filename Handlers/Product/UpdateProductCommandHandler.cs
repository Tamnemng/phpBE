using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Utilities;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";
    private const string CATEGORIES_KEY = "categories";
    private const string GIFTS_KEY = "gifts";

    public UpdateProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        // Get all products
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        // Find the product to update by ID
        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.Id).ToList();
        
        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with ID '{command.Id}' not found.");
        }

        // Validate brand
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME, 
            BRANDS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        if (!brands.Any(b => b.Code == command.BrandCode))
        {
            throw new InvalidOperationException($"Brand with code '{command.BrandCode}' does not exist.");
        }

        // Validate categories
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

        // Process gifts if specified
        List<Gift> updatedGifts = new List<Gift>();
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
                
                updatedGifts.Add(new Gift(giftMetadata.Name, giftMetadata.Code, giftMetadata.Image));
            }
        }

        // Update common properties for all product variants
        foreach (var product in productsToUpdate)
        {
            // Update basic product info while preserving Id and Code
            product.ProductInfo.Name = command.Name;
            product.ProductInfo.ImageUrl = command.ImageUrl;
            product.ProductInfo.Status = command.Status;
            product.ProductInfo.Brand = command.BrandCode;
            product.ProductInfo.Category = command.CategoriesCode;
            
            // Update gifts
            product.Gifts = updatedGifts;
            
            // Update price if specified
            if (command.Price != null)
            {
                product.Price.Update(command.Price);
            }
            
            // Update short description
            if (!string.IsNullOrWhiteSpace(command.ShortDescription))
            {
                product.ProductDetail.ShortDescription = command.ShortDescription;
            }
            
            // Update metadata
            product.UpdatedBy = command.UpdatedBy;
            product.UpdatedDate = DateTime.Now;
        }

        // Update variant-specific properties if needed
        if (command.Variants != null && command.Variants.Any())
        {
            UpdateProductVariants(productsToUpdate, command.Variants);
        }

        // Save updated products back to state store
        await _daprClient.SaveStateAsync(
            STORE_NAME, 
            PRODUCTS_KEY, 
            products, 
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }

    private void UpdateProductVariants(List<Product> products, List<VariantGroupUpdate> variantGroups)
    {
        // For each product variant
        foreach (var product in products)
        {
            // Get the product options
            foreach (var variantGroup in variantGroups)
            {
                // Find the corresponding option group in the product
                var productOptionGroup = product.ProductOptions.FirstOrDefault(po => 
                    po.Title.Equals(variantGroup.OptionTitle, StringComparison.OrdinalIgnoreCase));
                
                if (productOptionGroup != null)
                {
                    // For each option in the variant group
                    foreach (var variantOption in variantGroup.Options)
                    {
                        // Find the corresponding option in the product
                        var productOption = productOptionGroup.Options.FirstOrDefault(po => 
                            po.Label.Equals(variantOption.OptionLabel, StringComparison.OrdinalIgnoreCase));
                        
                        if (productOption != null)
                        {
                            // Update quantity
                            productOption.Quantity = variantOption.Quantity;
                            
                            // If this is the selected option for this product, update price and details
                            if (productOption.Selected)
                            {
                                // Update price if specified
                                if (variantOption.OriginalPrice.HasValue || variantOption.CurrentPrice.HasValue)
                                {
                                    var priceCommand = new UpdatePriceCommand
                                    {
                                        OriginalPrice = variantOption.OriginalPrice,
                                        DiscountPrice = variantOption.CurrentPrice
                                    };
                                    product.Price.Update(priceCommand);
                                }
                                
                                // Update barcode if specified
                                if (variantOption.Barcode.HasValue)
                                {
                                    product.ProductDetail.Barcode = variantOption.Barcode.Value;
                                }
                                
                                // Update descriptions if specified
                                if (variantOption.Descriptions != null && variantOption.Descriptions.Any())
                                {
                                    product.ProductDetail.Description = variantOption.Descriptions;
                                }
                                
                                // Update images if specified
                                if (variantOption.Images != null && variantOption.Images.Any())
                                {
                                    product.ProductDetail.Image = variantOption.Images;
                                }
                                
                                // Update short description if specified
                                if (!string.IsNullOrWhiteSpace(variantOption.ShortDescription))
                                {
                                    product.ProductDetail.ShortDescription = variantOption.ShortDescription;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}