using MediatR;

public class AddImageCollectionCommand : IRequest<Unit>
{
    public string Title { get; set; }
    public List<ImageItem> Images { get; set; }
    public string CreatedBy { get; set; }

    public AddImageCollectionCommand(string title, List<ImageItem> images, string createdBy)
    {
        Title = title;
        Images = images;
        CreatedBy = createdBy;
    }
}