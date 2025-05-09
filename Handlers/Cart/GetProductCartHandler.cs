using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class GetItemsDetailsHandler : IRequestHandler<GetItemsDetailsQuery, List<ItemDetailsResponseDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string COMBOS_KEY = "combos";

    public GetItemsDetailsHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<List<ItemDetailsResponseDto>> Handle(GetItemsDetailsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        if (request.Items.Count == 0)
        {
            return new List<ItemDetailsResponseDto>();
        }
        
        // Lấy danh sách sản phẩm từ state store
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        // Lấy danh sách combo từ state store
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();

        // Tạo dictionary sản phẩm để lookup nhanh cho combo products
        var productDictionary = new Dictionary<string, ProductSummaryDto>();
        foreach (var product in products)
        {
            if (!productDictionary.ContainsKey(product.ProductInfo.Code))
            {
                productDictionary.Add(product.ProductInfo.Code, new ProductSummaryDto
                {
                    Id = product.ProductInfo.Id,
                    Code = product.ProductInfo.Code,
                    Name = product.ProductInfo.Name,
                    ImageUrl = product.ProductInfo.ImageUrl,
                    CurrentPrice = product.Price.CurrentPrice,
                    OriginalPrice = product.Price.OriginalPrice,
                    ShortDescription = product.ProductDetail.ShortDescription
                });
            }
        }
        
        var result = new List<ItemDetailsResponseDto>();
        
        foreach (var item in request.Items)
        {
            var itemDetails = new ItemDetailsResponseDto
            {
                ItemId = item.ItemId,
                ItemType = item.ItemType
            };
            
            if (item.ItemType == CartItemType.Product)
            {
                var product = products.FirstOrDefault(p => p.ProductInfo.Id == item.ItemId);
                if (product != null)
                {
                    itemDetails.Name = product.ProductInfo.Name;
                    itemDetails.ImageUrl = product.ProductInfo.ImageUrl;
                    itemDetails.Price = product.Price.CurrentPrice;
                    
                    // Thêm discount percentage nếu sản phẩm có giảm giá
                    if (product.Price.DiscountPercentage > 0)
                    {
                        itemDetails.DiscountPercentage = product.Price.DiscountPercentage;
                    }
                    
                    result.Add(itemDetails);
                }
            }
            else if (item.ItemType == CartItemType.Combo)
            {
                var combo = combos.FirstOrDefault(c => c.Id == item.ItemId);
                if (combo != null)
                {
                    itemDetails.Name = combo.Name;
                    itemDetails.ImageUrl = combo.ImageUrl;
                    itemDetails.Price = combo.ComboPrice;
                    itemDetails.DiscountPercentage = combo.DiscountPercentage;
                    
                    // Thêm thông tin sản phẩm trong combo
                    foreach (var productCode in combo.ProductCodes)
                    {
                        if (productDictionary.TryGetValue(productCode, out var productDto))
                        {
                            itemDetails.ComboProducts.Add(productDto);
                        }
                    }
                    
                    result.Add(itemDetails);
                }
            }
        }
        
        return result;
    }
}