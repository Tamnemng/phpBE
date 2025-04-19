using System.ComponentModel.DataAnnotations;

public class AddBrandDto
{
    [Required]
    public string Code { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    // For base64 encoded image upload
    public string ImageBase64 { get; set; }
    
    public string CreatedBy { get; set; }
}

public class UpdateBrandDto
{
    [Required]
    public string Code { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    // For base64 encoded image upload
    public string ImageBase64 { get; set; }
    
    public string UpdatedBy { get; set; }
}