using System;
using System.Collections.Generic;
using OMS.Core.Utilities;

public class Product : BaseEntity
{

    public ProductInfo ProductInfo { get; set; }

    public Product()
    {
        ProductInfo = new ProductInfo();
    }


    public Product(AddProductCommand command) : base(command.CreatedBy)
    {
        ProductInfo = new ProductInfo
        {
            Id = IdGenerator.GenerateId(20), // Add ID generation similar to ProductInfo constructor
            Name = command.Name,
            Code = command.Code,
            ImageUrl = command.ImageUrl,
            Brand = new Brand
            {
                Name = command.Brand.Name,
                Code = command.Brand.Code,
                Logo = command.Brand.Logo
            },
            Category = command.Categories.Select(c => new Category
            {
                Name = c.Name,
                Code = c.Code
            }).ToList(), // Changed to ToList() to ensure it's not just an IEnumerable
            Status = command.Status
        };
    }

    public void Update(UpdateProductCommand command)
    {
        ProductInfo.Name = command.Name;
        ProductInfo.ImageUrl = command.ImageUrl;
        ProductInfo.Status = command.Status;
        ProductInfo.Brand = command.Brand;
        ProductInfo.Category = command.Categories;
    }

}