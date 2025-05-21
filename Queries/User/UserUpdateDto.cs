using System.ComponentModel.DataAnnotations;

public class UserUpdateDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? FullName { get; set; }

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
}