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
            Id = IdGenerator.GenerateId(20),
            Name = command.Name,
            Code = command.Code,
            ImageUrl = command.ImageUrl,
            Brand = command.BrandCode,
            Category = command.CategoriesCode,
            Status = command.Status
        };
    }

    public void Update(UpdateProductCommand command)
    {
        ProductInfo.Name = command.Name;
        ProductInfo.ImageUrl = command.ImageUrl;
        ProductInfo.Status = command.Status;
        ProductInfo.Brand = command.BrandId;
        ProductInfo.Category = command.CategoriesId;
    }

}