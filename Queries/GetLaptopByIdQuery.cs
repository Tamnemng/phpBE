
using MediatR;

public class GetLaptopByIdQuery : IRequest<Dictionary<string, object>>
{
    public string Id { get; set; }

    public GetLaptopByIdQuery(string id)
    {
        Id = id;
    }
}
