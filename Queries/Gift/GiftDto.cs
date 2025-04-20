using System.ComponentModel.DataAnnotations;

public class AddGiftDto
{
    [Required]
    public string Code { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    // For base64 encoded image upload
    public string ImageBase64 { get; set; }
}

public class UpdateGiftDto
{
    [Required]
    public string Code { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    // For base64 encoded image upload
    public string ImageBase64 { get; set; }
}