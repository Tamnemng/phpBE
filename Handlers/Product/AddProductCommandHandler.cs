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
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }

    public async Task<Unit> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        if (products.Any(p => p.ProductInfo.Code.Equals(command.Code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Product with code '{command.Code}' already exists.");
        }
        
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME, 
            BRANDS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        if (!brands.Any(b => b.Code == command.BrandCode))
        {
            throw new InvalidOperationException($"Brand with code '{command.BrandCode}' does not exist.");
        }

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

        // Process gift codes if provided
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
                
                command.Gifts.Add(new Gift(giftMetadata.Name, giftMetadata.Code, giftMetadata.Image));
            }
        }

        // Validate variants structure
        if (command.Variants == null || !command.Variants.Any() || 
            command.Variants.Any(v => v.Options == null || !v.Options.Any()))
        {
            throw new InvalidOperationException("At least one variant group with options is required.");
        }

        // Process and upload images for each variant (if they exist)
        foreach (var group in command.Variants)
        {
            foreach (var variant in group.Options)
            {
                var uploadedImages = new List<Image>();
                
                // Only process images if they exist
                if (variant.ImagesBase64 != null && variant.ImagesBase64.Any())
                {
                    foreach (var imageBase64 in variant.ImagesBase64)
                    {
                        if (!string.IsNullOrEmpty(imageBase64.Base64Content))
                        {
                            try
                            {
                                string imageUrl = await _cloudinaryService.UploadImageBase64Async(imageBase64.Base64Content);
                                
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
                }
                
                variant.Images = uploadedImages;
            }
        }

        // Check for variants that require images (e.g., color variants)
        bool hasImageVariant = false;
        foreach (var group in command.Variants)
        {
            // Check if this is an image-requiring variant (like color)
            bool isImageVariant = IsImageRequiringVariant(group.OptionTitle);
            
            if (isImageVariant)
            {
                hasImageVariant = true;
                
                // Verify all options in image-requiring variant groups have images
                foreach (var variant in group.Options)
                {
                    bool missingImages = variant.Images == null || !variant.Images.Any();
                    
                    if (missingImages)
                    {
                        throw new InvalidOperationException($"All '{group.OptionTitle}' variants must have at least one image.");
                    }
                }
            }
        }
        
        // Generate all possible combinations of variants
        var productVariants = GenerateProductVariantCombinations(command.Variants);
        
        foreach (var variantCombination in productVariants)
        {
            string productId = IdGenerator.GenerateId(20);
            
            // Create product for each combination
            var productVariation = new Product(command, productId, variantCombination.MainVariant);
            
            // Build product options
            var productOptions = new List<ProductOption>();
            
            // Add options for each variant group
            foreach (var group in command.Variants)
            {
                var options = new List<Option>();
                var selectedOptionLabel = variantCombination.SelectedOptions[group.OptionTitle];
                
                foreach (var variant in group.Options)
                {
                    bool isSelected = variant.OptionLabel == selectedOptionLabel;
                    options.Add(new Option(
                        variant.OptionLabel,
                        isSelected
                    ));
                }
                
                productOptions.Add(new ProductOption(group.OptionTitle, options));
            }
            
            productVariation.ProductOptions = productOptions;
            
            // Determine which variant to use for images
            // First, try to find an image-requiring variant group (like color)
            var imageVariantGroup = command.Variants.FirstOrDefault(g => IsImageRequiringVariant(g.OptionTitle));
            var allImages = new List<Image>();
            
            if (imageVariantGroup != null)
            {
                // Use the selected option from the image variant group for images
                var selectedImageOptionLabel = variantCombination.SelectedOptions[imageVariantGroup.OptionTitle];
                var selectedImageVariant = imageVariantGroup.Options.FirstOrDefault(v => v.OptionLabel == selectedImageOptionLabel);
                
                if (selectedImageVariant != null && selectedImageVariant.Images != null)
                {
                    allImages.AddRange(selectedImageVariant.Images);
                }
            }
            
            // Merge descriptions from all selected variants
            var allDescriptions = new List<Description>();
            
            // For each variant group in the combination
            foreach (var group in command.Variants)
            {
                // Get the selected option for this group
                string selectedOptionLabel = variantCombination.SelectedOptions[group.OptionTitle];
                var selectedVariant = group.Options.FirstOrDefault(o => o.OptionLabel == selectedOptionLabel);
                
                // Add descriptions from this variant
                if (selectedVariant != null && selectedVariant.Descriptions != null)
                {
                    allDescriptions.AddRange(selectedVariant.Descriptions);
                }
            }
            
            // Get short description from main variant (usually the first one)
            string shortDescription = variantCombination.MainVariant.ShortDescription;
            
            // Set the combined product details
            productVariation.ProductDetail = new ProductDetail(
                allDescriptions,
                allImages,
                shortDescription
            );
            
            // Always set price from the mainVariant
            productVariation.Price = Price.Create(variantCombination.MainVariant.OriginalPrice, variantCombination.MainVariant.CurrentPrice);
            
            products.Add(productVariation);
        }

        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);
        return Unit.Value;
    }

    // Helper method to determine if a variant type requires images
    // This is where you define which variant types need images (like color) and which don't (like storage)
    private bool IsImageRequiringVariant(string variantTitle)
    {
        // Add your logic to determine if the variant type requires images
        // For example, color variants need images, but storage capacity doesn't
        string title = variantTitle.ToLowerInvariant();
        return title.Contains("color") || title.Contains("màu") || title.Contains("colour");
    }

    // Helper class to store variant combination information
    private class VariantCombination
    {
        public ProductVariant MainVariant { get; set; }
        public Dictionary<string, string> SelectedOptions { get; set; } = new Dictionary<string, string>();
    }

    // Helper method to generate all possible combinations of variants
    private List<VariantCombination> GenerateProductVariantCombinations(List<VariantGroup> variantGroups)
    {
        var combinations = new List<VariantCombination>();
        
        // Start with first variant group
        var firstGroup = variantGroups.FirstOrDefault();
        if (firstGroup == null || !firstGroup.Options.Any())
        {
            throw new InvalidOperationException("At least one variant option is required.");
        }
        
        // Initialize combinations with first group
        foreach (var variant in firstGroup.Options)
        {
            var combination = new VariantCombination
            {
                MainVariant = variant,
                SelectedOptions = new Dictionary<string, string>
                {
                    { firstGroup.OptionTitle, variant.OptionLabel }
                }
            };
            
            combinations.Add(combination);
        }
        
        // Process additional variant groups
        foreach (var group in variantGroups.Skip(1))
        {
            var newCombinations = new List<VariantCombination>();
            
            foreach (var existing in combinations)
            {
                foreach (var option in group.Options)
                {
                    // Create a copy of the existing combination
                    var newCombination = new VariantCombination
                    {
                        // Keep the main variant the same as the original combination
                        MainVariant = existing.MainVariant,
                        SelectedOptions = new Dictionary<string, string>(existing.SelectedOptions)
                    };
                    
                    // Add the new option
                    newCombination.SelectedOptions[group.OptionTitle] = option.OptionLabel;
                    
                    newCombinations.Add(newCombination);
                }
            }
            
            combinations = newCombinations;
        }
        
        return combinations;
    }
}