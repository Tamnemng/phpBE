using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class GetProductTemplateQuery : IRequest<ProductCreateDto>
{
    public string Code { get; set; }

    public GetProductTemplateQuery(string code)
    {
        Code = code;
    }
}

public class GetProductTemplateQueryHandler : IRequestHandler<GetProductTemplateQuery, ProductCreateDto>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public GetProductTemplateQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<ProductCreateDto> Handle(GetProductTemplateQuery query, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        // Find all products with the requested code (should be multiple variants)
        var productVariants = products.Where(p => p.ProductInfo.Code.Equals(query.Code, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!productVariants.Any())
        {
            throw new KeyNotFoundException($"Product with code '{query.Code}' not found.");
        }

        // We'll use the first product as our base
        var baseProduct = productVariants.First();

        // Create a template that looks like the dto used to create the product
        var template = new ProductCreateDto
        {
            Name = baseProduct.ProductInfo.Name,
            Code = baseProduct.ProductInfo.Code,
            ImageBase64 = baseProduct.ProductInfo.ImageUrl, // Not using base64 for template
            CategoriesCode = baseProduct.ProductInfo.Category,
            Status = baseProduct.ProductInfo.Status,
            BrandCode = baseProduct.ProductInfo.Brand,
            GiftCodes = baseProduct.Gifts?.Select(g => g.Code)?.ToList() ?? new List<string>(),
            Variants = new List<VariantGroupDto>()
        };

        // Reconstruct variant groups
        var allVariantGroups = new Dictionary<string, VariantGroupDto>();

        // Process each product to extract variant information
        foreach (var product in productVariants)
        {
            if (product.ProductOptions != null)
            {
                foreach (var optionGroup in product.ProductOptions)
                {
                    string optionTitle = optionGroup.Title;

                    // Create or get variant group
                    if (!allVariantGroups.TryGetValue(optionTitle, out var variantGroup))
                    {
                        variantGroup = new VariantGroupDto
                        {
                            OptionTitle = optionTitle,
                            Options = new List<ProductVariantDto>()
                        };
                        allVariantGroups[optionTitle] = variantGroup;
                    }

                    // Add the selected option for this product
                    var selectedOption = optionGroup.Options.FirstOrDefault(o => o.Selected);
                    if (selectedOption != null)
                    {
                        // Check if this option already exists in our collection
                        if (!variantGroup.Options.Any(o => o.OptionLabel == selectedOption.Label))
                        {
                            // Add this variant option
                            var variantDto = new ProductVariantDto
                            {
                                OptionLabel = selectedOption.Label,
                                OriginalPrice = product.Price.OriginalPrice,
                                CurrentPrice = product.Price.CurrentPrice,
                                ShortDescription = product.ProductDetail?.ShortDescription ?? string.Empty,
                                Descriptions = product.ProductDetail?.Description?.ToList() ?? new List<Description>(),
                                ImagesBase64 = new List<ImageBase64Dto>()
                            };

                            // Keep original image URLs
                            if (product.ProductDetail?.Image != null)
                            {
                                variantDto.ImagesBase64 = product.ProductDetail.Image
                                    .Select(img => new ImageBase64Dto
                                    {
                                        Base64Content = img.Url,
                                        Priority = img.piority
                                    })
                                    .ToList();
                            }

                            variantGroup.Options.Add(variantDto);
                        }
                    }
                }
            }
        }

        // Add all variant groups to the template
        template.Variants = allVariantGroups.Values.ToList();

        return template;
    }
}