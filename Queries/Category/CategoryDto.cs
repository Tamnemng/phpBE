using System.ComponentModel.DataAnnotations;

public class AddCategoryDto
{
    [Required]
    public string Code { get; set; }
    
    [Required]
    public string Name { get; set; }
    
}

public class UpdateCategoryDto
{
    [Required]
    public string Id { get; set; }
    
    [Required]
    public string Name { get; set; }
}