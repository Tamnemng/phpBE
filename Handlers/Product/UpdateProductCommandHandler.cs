using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Utilities;
using Think4.Services;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private readonly ICloudinaryService _cloudinaryService;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";
    private const string CATEGORIES_KEY = "categories";
    private const string GIFTS_KEY = "gifts";

    public UpdateProductCommandHandler(DaprClient daprClient, ICloudinaryService cloudinaryService)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }

    public async Task<Unit> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.Id).ToList();
        
        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with ID '{command.Id}' not found.");
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

        if (command.Variants != null && command.Variants.Any())
        {
            foreach (var group in command.Variants)
            {
                foreach (var variant in group.Options)
                {
                    if (variant.ImagesBase64 != null && variant.ImagesBase64.Any())
                    {
                        var uploadedImages = new List<Image>();
                        
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
                        
                        variant.Images = uploadedImages;
                    }
                }
            }
        }

        foreach (var product in productsToUpdate)
        {
            product.ProductInfo.Name = command.Name;
            product.ProductInfo.ImageUrl = command.ImageUrl;
            product.ProductInfo.Status = command.Status;
            product.ProductInfo.Brand = command.BrandCode;
            product.ProductInfo.Category = command.CategoriesCode;
            
            product.Gifts = updatedGifts;
            
            if (command.Price != null)
            {
                product.Price.Update(command.Price);
            }
            
            if (!string.IsNullOrWhiteSpace(command.ShortDescription))
            {
                product.ProductDetail.ShortDescription = command.ShortDescription;
            }
            
            product.UpdatedBy = command.UpdatedBy;
            product.UpdatedDate = DateTime.Now;
        }

        if (command.Variants != null && command.Variants.Any())
        {
            UpdateProductVariants(productsToUpdate, command.Variants);
        }

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
        foreach (var product in products)
        {
            foreach (var variantGroup in variantGroups)
            {
                var productOptionGroup = product.ProductOptions.FirstOrDefault(po => 
                    po.Title.Equals(variantGroup.OptionTitle, StringComparison.OrdinalIgnoreCase));
                
                if (productOptionGroup != null)
                {
                    foreach (var variantOption in variantGroup.Options)
                    {
                        var productOption = productOptionGroup.Options.FirstOrDefault(po => 
                            po.Label.Equals(variantOption.OptionLabel, StringComparison.OrdinalIgnoreCase));
                        
                        if (productOption != null)
                        {
                            productOption.Quantity = variantOption.Quantity;
                            
                            if (productOption.Selected)
                            {
                                if (variantOption.OriginalPrice.HasValue || variantOption.CurrentPrice.HasValue)
                                {
                                    var priceCommand = new UpdatePriceCommand
                                    {
                                        OriginalPrice = variantOption.OriginalPrice,
                                        DiscountPrice = variantOption.CurrentPrice
                                    };
                                    product.Price.Update(priceCommand);
                                }
                                
                                if (variantOption.Barcode.HasValue)
                                {
                                    product.ProductDetail.Barcode = variantOption.Barcode.Value;
                                }
                                
                                if (variantOption.Descriptions != null && variantOption.Descriptions.Any())
                                {
                                    product.ProductDetail.Description = variantOption.Descriptions;
                                }
                                
                                if (variantOption.Images != null && variantOption.Images.Any())
                                {
                                    product.ProductDetail.Image = variantOption.Images;
                                }
                                
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