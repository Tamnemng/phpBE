using System.Text.Json.Serialization;
using MediatR;
using System.Collections.Generic;

public class AddProductCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string ImageUrl { get; set; } // Will be populated from ImageBase64 after upload
    public IEnumerable<string> CategoriesCode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    public string BrandCode { get; set; }
    public string CreatedBy { get; set; }
    public List<Gift> Gifts { get; set; }
    public List<string> GiftCodes { get; set; }
    public List<VariantGroup> Variants { get; set; }

    public AddProductCommand() 
    {
        Variants = new List<VariantGroup>();
        CategoriesCode = new List<string>();
        Gifts = new List<Gift>();
        GiftCodes = new List<string>();
    }

    public AddProductCommand(ProductCreateDto dto)
    {
        Name = dto.Name;
        Code = dto.Code;
        ImageUrl = ""; // Will be set after Cloudinary upload in handler
        CategoriesCode = dto.CategoriesCode ?? new List<string>();
        BrandCode = dto.BrandCode;
        Status = dto.Status;
        CreatedBy = dto.CreatedBy;
        GiftCodes = dto.GiftCodes ?? new List<string>();
        Gifts = new List<Gift>();
        Variants = new List<VariantGroup>();
        
        if (dto.Variants != null)
        {
            foreach (var variantGroup in dto.Variants)
            {
                var group = new VariantGroup
                {
                    OptionTitle = variantGroup.OptionTitle,
                    Options = new List<ProductVariant>()
                };
                
                foreach (var optionDto in variantGroup.Options)
                {
                    var variant = new ProductVariant
                    {
                        OptionLabel = optionDto.OptionLabel,
                        Quantity = optionDto.Quantity,
                        OriginalPrice = optionDto.OriginalPrice,
                        CurrentPrice = optionDto.CurrentPrice,
                        Barcode = optionDto.Barcode,
                        Descriptions = optionDto.Descriptions ?? new List<Description>(),
                        Images = new List<Image>(), // Will be populated from ImagesBase64 after upload
                        ImagesBase64 = optionDto.ImagesBase64,
                        ShortDescription = optionDto.ShortDescription
                    };
                    
                    group.Options.Add(variant);
                }
                
                Variants.Add(group);
            }
        }
    }
}

public class VariantGroup
{
    public string OptionTitle { get; set; }
    public List<ProductVariant> Options { get; set; } = new List<ProductVariant>();
}

public class ProductVariant
{
    public string OptionLabel { get; set; }
    public int Quantity { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public int Barcode { get; set; }
    public IEnumerable<Description> Descriptions { get; set; }
    public IEnumerable<Image> Images { get; set; }
    public IEnumerable<ImageBase64Dto> ImagesBase64 { get; set; }
    public string ShortDescription { get; set; }
}