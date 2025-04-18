using System.Text.Json.Serialization;
using MediatR;
using System.Collections.Generic;

public class AddProductCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string ImageUrl { get; set; }
    public IEnumerable<string> CategoriesCode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    public string BrandCode { get; set; }
    public string CreatedBy { get; set; }
    
    public List<ProductVariant> Variants { get; set; }

    public AddProductCommand() 
    {
        Variants = new List<ProductVariant>();
        CategoriesCode = new List<string>();
    }

    public AddProductCommand(ProductCreateDto dto)
    {
        Name = dto.Name;
        Code = dto.Code;
        ImageUrl = dto.ImageUrl;
        CategoriesCode = dto.CategoriesCode ?? new List<string>();
        BrandCode = dto.BrandCode;
        Status = dto.Status;
        CreatedBy = dto.CreatedBy;
        
        Variants = new List<ProductVariant>();
        if (dto.Variants != null)
        {
            foreach (var variantDto in dto.Variants)
            {
                Variants.Add(new ProductVariant
                {
                    OptionTitle = variantDto.OptionTitle,
                    OptionLabel = variantDto.OptionLabel,
                    Quantity = variantDto.Quantity,
                    OriginalPrice = variantDto.OriginalPrice,
                    CurrentPrice = variantDto.CurrentPrice,
                    Barcode = variantDto.Barcode,
                    Descriptions = variantDto.Descriptions ?? new List<Description>(),
                    Images = variantDto.Images ?? new List<Image>(),
                    ShortDescription = variantDto.ShortDescription
                });
            }
        }
    }
}

public class ProductVariant
{
    public string OptionTitle { get; set; }
    public string OptionLabel { get; set; }
    public int Quantity { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public int Barcode { get; set; }
    public IEnumerable<Description> Descriptions { get; set; }
    public IEnumerable<Image> Images { get; set; }
    public string ShortDescription { get; set; }
}