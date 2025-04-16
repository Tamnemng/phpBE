using System.Text.Json.Serialization;
using MediatR;

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

    public AddProductCommand(string name, string code, string imageUrl, ProductStatus status, IEnumerable<string> categoriesCode, string brandCode, string createdBy)
    {
        Name = name;
        Code = code;
        ImageUrl = imageUrl;
        CategoriesCode = categoriesCode;
        BrandCode = brandCode;
        Status = status;
        CreatedBy = createdBy;
    }
}