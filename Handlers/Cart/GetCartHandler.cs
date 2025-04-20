using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class CartResponseDto
{
    public string UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    
    public CartResponseDto(string userId)
    {
        UserId = userId;
    }
}

public class CartItemDto
{
    public string ItemId { get; set; }
    public CartItemType ItemType { get; set; }
    public int Quantity { get; set; }
    
    // Additional information based on type
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public List<ProductSummaryDto> ComboProducts { get; set; } = new List<ProductSummaryDto>();
    
    public CartItemDto(CartItem item)
    {
        ItemId = item.ItemId;
        ItemType = item.ItemType;
        Quantity = item.Quantity;
    }
}

public class GetCartHandler : IRequestHandler<GetCartQuery, CartResponseDto>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string CART_METADATA_KEY = "carts";
    private const string PRODUCTS_KEY = "products";
    private const string COMBOS_KEY = "combos";

    public GetCartHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<CartResponseDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        
        var cartList = await _daprClient.GetStateAsync<List<Cart>>(
            STORE_NAME,
            CART_METADATA_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Cart>();

        var existingCart = cartList.FirstOrDefault(c => c.UserId == request.userId);
        
        // If cart doesn't exist, return empty cart
        if (existingCart == null)
        {
            return new CartResponseDto(request.userId);
        }
        
        // Get products and combos to enrich cart items
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        var combos = await _daprClient.GetStateAsync<List<Combo>>(
            STORE_NAME,
            COMBOS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Combo>();

        var response = new CartResponseDto(existingCart.UserId);
        
        // Create product dictionary for faster lookups in combo products
        var productDictionary = products.ToDictionary(
            p => p.ProductInfo.Code,
            p => new ProductSummaryDto
            {
                Id = p.ProductInfo.Id,
                Code = p.ProductInfo.Code,
                Name = p.ProductInfo.Name,
                ImageUrl = p.ProductInfo.ImageUrl,
                CurrentPrice = p.Price.CurrentPrice,
                OriginalPrice = p.Price.OriginalPrice,
                ShortDescription = p.ProductDetail.ShortDescription
            });
            
        foreach (var item in existingCart.Items)
        {
            var cartItemDto = new CartItemDto(item);
            
            if (item.ItemType == CartItemType.Product)
            {
                var product = products.FirstOrDefault(p => p.ProductInfo.Id == item.ItemId);
                if (product != null)
                {
                    cartItemDto.Name = product.ProductInfo.Name;
                    cartItemDto.ImageUrl = product.ProductInfo.ImageUrl;
                    cartItemDto.Price = product.Price.CurrentPrice;
                    cartItemDto.TotalPrice = product.Price.CurrentPrice * item.Quantity;
                    
                    // Add discount percentage if product has a discount
                    if (product.Price.DiscountPercentage > 0)
                    {
                        cartItemDto.DiscountPercentage = product.Price.DiscountPercentage;
                    }
                }
            }
            else if (item.ItemType == CartItemType.Combo)
            {
                var combo = combos.FirstOrDefault(c => c.Id == item.ItemId);
                if (combo != null)
                {
                    cartItemDto.Name = combo.Name;
                    cartItemDto.ImageUrl = combo.ImageUrl;
                    cartItemDto.Price = combo.ComboPrice;
                    cartItemDto.TotalPrice = combo.ComboPrice * item.Quantity;
                    cartItemDto.DiscountPercentage = combo.DiscountPercentage;
                    
                    // Add products in the combo
                    foreach (var productCode in combo.ProductCodes)
                    {
                        if (productDictionary.TryGetValue(productCode, out var productDto))
                        {
                            cartItemDto.ComboProducts.Add(productDto);
                        }
                    }
                }
            }
            
            response.Items.Add(cartItemDto);
        }
        
        return response;
    }
}