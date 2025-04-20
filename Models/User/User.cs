// Models/User/User.cs
using OMS.Core.Utilities;

public enum UserRole
{
    User,
    Manager,
    Admin
}

public class User : BaseEntity
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // Should store hashed password in real app
    public string Email { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastActive { get; set; }
    public UserRole Role { get; set; }

    public User() : base()
    {
        Id = IdGenerator.GenerateId(16);
        Username = string.Empty;
        Password = string.Empty;
        Email = string.Empty;
        FullName = string.Empty;
        IsActive = true;
        Role = UserRole.User;
    }

    public User(string username, string password, string email, string fullName, UserRole role = UserRole.User) 
        : base(username)
    {
        Id = IdGenerator.GenerateId(16);
        Username = username;
        Password = password;  // This should be hashed in a real app
        Email = email;
        FullName = fullName;
        IsActive = true;
        Role = role;
        LastActive = DateTime.UtcNow;
    }

    public void UpdateLastActive()
    {
        LastActive = DateTime.UtcNow;
    }

    public bool IsSessionExpired(TimeSpan expirationTime)
    {
        if (!LastActive.HasValue)
            return true;

        return DateTime.UtcNow - LastActive.Value > expirationTime;
    }
}