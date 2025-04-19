using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Utilities;
using Think4.Services;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private readonly ICloudinaryService _cloudinaryService;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";
    private const string CATEGORIES_KEY = "categories";
    private const string GIFTS_KEY = "gifts";

    public AddProductCommandHandler(DaprClient daprClient, ICloudinaryService cloudinaryService)
    {
        _daprClient = daprClient;
        _cloudinaryService = cloudinaryService;
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

        // Process gifts if specified
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

        // Process variants with new structure
        if (command.Variants != null && command.Variants.Any())
        {
            // Ensure we have at least one variant group
            var firstGroup = command.Variants.FirstOrDefault();
            if (firstGroup == null || !firstGroup.Options.Any())
            {
                throw new InvalidOperationException("At least one variant option is required.");
            }
            
            // Process base64 images for each variant
            foreach (var group in command.Variants)
            {
                foreach (var variant in group.Options)
                {
                    if (variant.ImagesBase64 != null && variant.ImagesBase64.Any())
                    {
                        var uploadedImages = new List<Image>();
                        
                        // Keep existing images
                        if (variant.Images != null)
                        {
                            uploadedImages.AddRange(variant.Images);
                        }
                        
                        // Process and upload each base64 image
                        foreach (var imageBase64 in variant.ImagesBase64)
                        {
                            if (!string.IsNullOrEmpty(imageBase64.Base64Content))
                            {
                                try
                                {
                                    // Upload to Cloudinary
                                    string imageUrl = await _cloudinaryService.UploadImageBase64Async(imageBase64.Base64Content);
                                    
                                    // Add to images list
                                    uploadedImages.Add(new Image
                                    {
                                        Url = imageUrl,
                                        piority = imageBase64.Priority
                                    });
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidOperationException($"Failed to upload image: {ex.Message}");
                                }
                            }
                        }
                        
                        // Replace the images collection with our new one that includes uploaded images
                        variant.Images = uploadedImages;
                    }
                }
            }
            
            // Generate IDs for each option in the first variant group
            var optionIdMap = new Dictionary<string, string>();
            foreach (var variant in firstGroup.Options)
            {
                optionIdMap[variant.OptionLabel] = IdGenerator.GenerateId(20);
            }
            
            // Create a product for each option in the first variant group
            foreach (var variant in firstGroup.Options)
            {
                string productId = optionIdMap[variant.OptionLabel];
                var productVariation = new Product(command, productId, variant);
                
                // Create product options for the current product
                var productOptions = new List<ProductOption>();
                
                // Add options from the first group (main options)
                var mainOptions = new List<Option>();
                foreach (var optionLabel in optionIdMap.Keys)
                {
                    bool isSelected = optionLabel == variant.OptionLabel;
                    var optionVariant = firstGroup.Options.FirstOrDefault(v => v.OptionLabel == optionLabel);
                    mainOptions.Add(new Option(
                        optionLabel,
                        optionIdMap[optionLabel],
                        optionVariant?.Quantity ?? 0,
                        isSelected
                    ));
                }
                productOptions.Add(new ProductOption(firstGroup.OptionTitle, mainOptions));
                
                // Add options from other variant groups
                foreach (var group in command.Variants.Skip(1))
                {
                    var options = new List<Option>();
                    foreach (var otherVariant in group.Options)
                    {
                        options.Add(new Option(
                            otherVariant.OptionLabel,
                            IdGenerator.GenerateId(20),
                            otherVariant.Quantity,
                            false
                        ));
                    }
                    productOptions.Add(new ProductOption(group.OptionTitle, options));
                }
                
                productVariation.ProductOptions = productOptions;
                products.Add(productVariation);
            }
        }
        else
        {
            throw new InvalidOperationException("At least one variant group with options is required.");
        }

        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}