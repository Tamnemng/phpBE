using MediatR;
public class UpdateImageCollectionCommand : IRequest<Unit>
{
    public string Id { get; set; }
    public string Title { get; set; }
    public List<ImageItem> NewImages { get; set; }
    public List<ImageUpdateItem> UpdatedImages { get; set; }
    public List<string> DeletedImageUrls { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateImageCollectionCommand(string id, string title, List<ImageItem> newImages, 
        List<ImageUpdateItem> updatedImages, List<string> deletedImageUrls, string updatedBy)
    {
        Id = id;
        Title = title;
        NewImages = newImages ?? new List<ImageItem>();
        UpdatedImages = updatedImages ?? new List<ImageUpdateItem>();
        DeletedImageUrls = deletedImageUrls ?? new List<string>();
        UpdatedBy = updatedBy;
    }
}