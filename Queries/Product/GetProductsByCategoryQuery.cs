using MediatR;

public class GetProductsByCategoryQuery : IRequest<List<Product>>
{
    public string CategoryId { get; }

    public GetProductsByCategoryQuery(string categoryId)
    {
        CategoryId = categoryId;
    }
}