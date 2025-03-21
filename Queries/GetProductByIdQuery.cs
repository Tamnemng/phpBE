
using MediatR;

public class GetProductByIdQuery : IRequest<Dictionary<string, object>>
{
    public string Id { get; set; }

    public GetProductByIdQuery(string id)
    {
        Id = id;
    }
}
