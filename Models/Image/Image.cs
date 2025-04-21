using OMS.Core.Utilities;

public class ImageCollection
{
    public string Id { get; set; }
    public string Title { get; set; }
    public List<ImageItem> Images { get; set; }

    public ImageCollection()
    {
        Id = IdGenerator.GenerateId(16);
        Title = string.Empty;
        Images = new List<ImageItem>();
    }
}

public class ImageItem
{
    public string Url { get; set; }
    public int Priority { get; set; }

    public ImageItem()
    {
        Url = string.Empty;
        Priority = 0;
    }

    public ImageItem(string url, int priority)
    {
        Url = url;
        Priority = priority;
    }
}