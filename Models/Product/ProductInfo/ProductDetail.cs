
public class Image
{
    public string Url { get; set; }
    public int piority { get; set; }
    public Image()
    {
        Url = string.Empty;
        piority = 0;
    }

    public Image(string url, int piority)
    {
        Url = url;
        this.piority = piority;
    }
    public override string ToString()
    {
        return $"Url: {Url}, Piority: {piority}";
    }
}
// public class ImageContainer
// {
//     public IEnumerable<Image> Images { get; set; }
//     public string color { get; set; }
//     public ImageContainer()
//     {
//         Images = new List<Image>();
//         color = string.Empty;
//     }
//     public ImageContainer(IEnumerable<Image> images, string color)
//     {
//         Images = images;
//         this.color = color;
//     }
//     public ImageContainer(IEnumerable<Image> images)
//     {
//         Images = images;
//         color = string.Empty;
//     }
//     public override string ToString()
//     {
//         return $"Images: {string.Join(", ", Images)}";
//     }
// }
public class Description
{
    public string Name { get; set; }
    public string DescriptionText { get; set; }
    public int Priority { get; set; }
    public Description()
    {
        Name = string.Empty;
        DescriptionText = string.Empty;
        Priority = 0;
    }
    public Description(string name, string descriptionText, int priority)
    {
        Name = name;
        DescriptionText = descriptionText;
        Priority = priority;
    }
    public override string ToString()
    {
        return $"Name: {Name}, DescriptionText: {DescriptionText}, Piority: {Priority}";
    }
}
public class ProductDetail
{
    public int Barcode { get; set; }
    public IEnumerable<Description> Description { get; set; }
    public IEnumerable<Image> Image { get; set; }
    public string ShortDescription { get; set; }
    public ProductDetail()
    {
        Barcode = 0;
        Description = new List<Description>();
        Image = new List<Image>();
        ShortDescription = string.Empty;
    }
    public ProductDetail(int barcode, IEnumerable<Description> description, IEnumerable<Image> image, string shortDescription)
    {
        Barcode = barcode;
        Description = description;
        Image = image;
        ShortDescription = shortDescription;
    }
}