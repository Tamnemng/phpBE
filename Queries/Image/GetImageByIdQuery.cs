using MediatR;
public class GetImageCollectionByIdQuery : IRequest<ImageCollection>
{
    public string Id { get; }

    public GetImageCollectionByIdQuery(string id)
    {
        Id = id;
    }
}