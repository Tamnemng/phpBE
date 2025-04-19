public class ImageBase64Dto
{
    public string Base64Content { get; set; }
    public int Priority { get; set; }
    
    public ImageBase64Dto()
    {
        Base64Content = string.Empty;
        Priority = 0;
    }
    
    public ImageBase64Dto(string base64Content, int priority)
    {
        Base64Content = base64Content;
        Priority = priority;
    }
}