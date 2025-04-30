using System.Text.Json.Serialization;
using MediatR;
using System.Collections.Generic;

public class UpdateProductCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public IEnumerable<string> CategoriesCode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductStatus Status { get; set; }
    public string BrandCode { get; set; }
    public string UpdatedBy { get; set; }
    public UpdatePriceCommand Price { get; set; }
    public List<Gift> Gifts { get; set; } = new List<Gift>();
    public List<string> GiftCodes { get; set; } = new List<string>();
    public List<VariantGroupUpdate> Variants { get; set; } = new List<VariantGroupUpdate>();
    public string ShortDescription { get; set; }

    public UpdateProductCommand() 
    {
        CategoriesCode = new List<string>();
    }

    public UpdateProductCommand(ProductUpdateDto dto, string username)
    {
        Id = dto.Id;
        Name = dto.Name;
        ImageUrl = dto.ImageUrl; // This will be updated in controller if ImageBase64 is provided
        CategoriesCode = dto.CategoriesCode ?? new List<string>();
        BrandCode = dto.BrandCode;
        Status = dto.Status;
        UpdatedBy = username;
        Price = dto.Price;
        GiftCodes = dto.GiftCodes ?? new List<string>();
        
        if (dto.Variants != null)
        {
            foreach (var groupDto in dto.Variants)
            {
                var group = new VariantGroupUpdate
                {
                    OptionTitle = groupDto.OptionTitle,
                    Options = new List<ProductVariantUpdate>()
                };
                
                foreach (var optionDto in groupDto.Options)
                {
                    group.Options.Add(new ProductVariantUpdate
                    {
                        OptionLabel = optionDto.OptionLabel,
                        OriginalPrice = optionDto.OriginalPrice,
                        CurrentPrice = optionDto.CurrentPrice,
                        Descriptions = optionDto.Descriptions,
                        Images = null, // Will be populated from ImagesBase64 after upload
                        ImagesBase64 = optionDto.ImagesBase64,
                        ShortDescription = optionDto.ShortDescription
                    });
                }
                
                Variants.Add(group);
            }
        }
    }
}

public class VariantGroupUpdate
{
    public string OptionTitle { get; set; }
    public List<ProductVariantUpdate> Options { get; set; } = new List<ProductVariantUpdate>();
}

public class ProductVariantUpdate
{
    public string OptionLabel { get; set; }
    public int Quantity { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public int? Barcode { get; set; }
    public IEnumerable<Description> Descriptions { get; set; }
    public IEnumerable<Image> Images { get; set; }
    public IEnumerable<ImageBase64Dto> ImagesBase64 { get; set; }
    public string ShortDescription { get; set; }
}