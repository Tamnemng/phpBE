// Models/User/UserDto.cs
using System.ComponentModel.DataAnnotations;

public class UserLoginDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}

public class UserRegisterDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string FullName { get; set; }
}

public class ManagerRegisterDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string FullName { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public DateTime? LastActive { get; set; }
    
    public UserDto(User user)
    {
        Id = user.Id;
        Username = user.Username;
        Email = user.Email;
        FullName = user.FullName;
        Role = user.Role;
        LastActive = user.LastActive;
    }
}

public class LoginResponseDto
{
    public string Token { get; set; }
    public UserDto User { get; set; }
    public DateTime Expiration { get; set; }
}