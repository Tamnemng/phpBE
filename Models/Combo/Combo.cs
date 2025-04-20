using OMS.Core.Utilities;
using System;
using System.Collections.Generic;

public class Combo : BaseEntity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<string> ProductCodes { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; }

    public Combo() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Name = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
        ProductCodes = new List<string>();
        IsActive = true;
    }

    public Combo(string name, string description, string imageUrl, List<string> productCodes, decimal comboPrice, string createdBy)
        : base(createdBy)
    {
        Id = IdGenerator.GenerateId(16);
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        ProductCodes = productCodes;
        ComboPrice = comboPrice;
        IsActive = true;
        
        // Will be calculated when actual products are processed
        OriginalPrice = 0;
        DiscountPercentage = 0;
    }

    public void UpdateDiscountInfo(decimal originalPrice)
    {
        OriginalPrice = originalPrice;
        if (originalPrice > 0)
        {
            DiscountPercentage = Math.Round(100 - (ComboPrice / originalPrice * 100), 2);
        }
        else
        {
            DiscountPercentage = 0;
        }
    }

    public void Update(UpdateComboCommand command, string updatedBy)
    {
        base.Update(updatedBy);
        
        if (!string.IsNullOrEmpty(command.Name))
            Name = command.Name;
            
        if (!string.IsNullOrEmpty(command.Description))
            Description = command.Description;
            
        if (!string.IsNullOrEmpty(command.ImageUrl))
            ImageUrl = command.ImageUrl;
            
        if (command.ProductCodes != null && command.ProductCodes.Count > 0)
            ProductCodes = command.ProductCodes;
            
        if (command.ComboPrice.HasValue)
            ComboPrice = command.ComboPrice.Value;
            
        if (command.IsActive.HasValue)
            IsActive = command.IsActive.Value;
    }
}