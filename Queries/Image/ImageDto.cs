using System.ComponentModel.DataAnnotations;

public class AddImageCollectionDto
{
    [Required]
    public string Title { get; set; }
    
    [Required]
    public List<ImageBase64Item> Images { get; set; }
}

public class ImageBase64Item
{
    [Required]
    public string ImageBase64 { get; set; }
    
    public int Priority { get; set; }
}

public class UpdateImageCollectionDto
{
    [Required]
    public string Id { get; set; }
    
    public string Title { get; set; }
    
    public List<ImageBase64Item> NewImages { get; set; }
    
    public List<ImageUpdateItem> UpdatedImages { get; set; }
    
    public List<string> DeletedImageUrls { get; set; }
}

public class ImageUpdateItem
{
    [Required]
    public string Url { get; set; }
    
    public int Priority { get; set; }
}