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

    public AddProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken)
            ?? new List<Product>();
        
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

        if (command.Variants != null && command.Variants.Any())
        {
            // Nhóm các biến thể theo tiêu đề tùy chọn (ví dụ: "Color", "Size", vv)
            var variantsByOption = command.Variants
                .GroupBy(v => v.OptionTitle)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Lấy tiêu đề tùy chọn đầu tiên để tạo các biến thể sản phẩm
            var firstOptionTitle = variantsByOption.Keys.FirstOrDefault();
            
            if (firstOptionTitle != null)
            {
                // Pre-generate IDs for all options to ensure consistency
                var optionIdMap = new Dictionary<string, string>();
                foreach (var variant in variantsByOption[firstOptionTitle])
                {
                    // Generate a unique ID for each option label
                    optionIdMap[variant.OptionLabel] = IdGenerator.GenerateId(20);
                }
                
                // Tạo sản phẩm cho mỗi biến thể của tùy chọn đầu tiên
                foreach (var variant in variantsByOption[firstOptionTitle])
                {
                    // Sử dụng ID đã tạo trước cho tùy chọn này
                    string productId = optionIdMap[variant.OptionLabel];
                    
                    // Tạo sản phẩm với constructor mới
                    var productVariation = new Product(command, productId, variant);
                    
                    // Tạo các tùy chọn sản phẩm
                    var productOptions = new List<ProductOption>();
                    
                    // Thêm tùy chọn chính (như màu sắc)
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
                    
                    // Thêm các tùy chọn khác nếu có
                    foreach (var optionTitle in variantsByOption.Keys.Where(k => k != firstOptionTitle))
                    {
                        var options = new List<Option>();
                        foreach (var otherVariant in variantsByOption[optionTitle])
                        {
                            options.Add(new Option(
                                otherVariant.OptionLabel,
                                IdGenerator.GenerateId(20),
                                otherVariant.Quantity,
                                false // Mặc định không được chọn
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
            // Không có biến thể, tạo một sản phẩm đơn lẻ
            throw new InvalidOperationException("Phải cung cấp ít nhất một biến thể sản phẩm.");
        }

        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}