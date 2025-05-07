using OMS.Core.Utilities;
using System;

public class Price
{
    
    public decimal OriginalPrice { get; set; }
    
    public decimal CurrentPrice { get; set; }
    
    public decimal DiscountPrice { get; set; }
    
    public decimal DiscountPercentage { get; set; }
    
    public Price()
    {
    }

    public Price(decimal originalPrice, decimal currentPrice)
    {
        OriginalPrice = originalPrice;
        CurrentPrice = currentPrice;
        DiscountPrice = currentPrice;
        DiscountPercentage = CalculateDiscountPercentage(originalPrice, currentPrice);
    }

    public static Price Create(decimal originalPrice, decimal currentPrice)
    {
        return new Price
        {
            OriginalPrice = originalPrice,
            CurrentPrice = currentPrice,
            DiscountPrice = currentPrice,
            DiscountPercentage = CalculateDiscountPercentage(originalPrice, currentPrice)
        };
    }

    public void ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
        {
            throw new ArgumentException("Discount percentage must be between 0 and 100");
        }

        DiscountPercentage = discountPercentage;
        DiscountPrice = Math.Round(OriginalPrice * (1 - (discountPercentage / 100)), 2);
        CurrentPrice = DiscountPrice;
    }

    public void ApplyFixedPrice(decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new ArgumentException("Price cannot be negative");
        }

        DiscountPrice = newPrice;
        CurrentPrice = newPrice;
        DiscountPercentage = CalculateDiscountPercentage(OriginalPrice, newPrice);
    }

    public void Update(UpdatePriceCommand command)
    {
        if (command.OriginalPrice.HasValue)
            OriginalPrice = command.OriginalPrice.Value;
        
        if (command.DiscountPercentage.HasValue)
            ApplyDiscount(command.DiscountPercentage.Value);
        else if (command.DiscountPrice.HasValue)
            ApplyFixedPrice(command.DiscountPrice.Value);
    }

    private static decimal CalculateDiscountPercentage(decimal originalPrice, decimal currentPrice)
    {
        if (originalPrice <= 0)
            return 0;
            
        decimal percentage = 100 - (currentPrice / originalPrice * 100);
        return Math.Round(percentage, 2);
    }

    public void Update(decimal originalPrice, decimal currentPrice)
    {
        OriginalPrice = originalPrice;
        CurrentPrice = currentPrice;
        DiscountPercentage = CalculateDiscountPercentage(originalPrice, currentPrice);
    }
}

public class UpdatePriceCommand
{
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
}