
using MediatR;

public class GetLaptopByIdQuery : IRequest<string>
{
    public string Id {get; set;}

    public GetLaptopByIdQuery(string id){
        Id = id;
    }
}