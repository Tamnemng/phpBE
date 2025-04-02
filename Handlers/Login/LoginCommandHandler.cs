using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

// Lưu trữ thông tin đăng nhập dựa trên IP
public class UserIpSession
{
    public string Username { get; set; }
    public string IpAddress { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime ExpiryTime { get; set; }
    public bool IsActive => DateTime.UtcNow < ExpiryTime;
}

// Lưu trữ thông tin người dùng, bao gồm danh sách IP đã đăng nhập
public class UserData
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public List<UserIpSession> IpSessions { get; set; } = new List<UserIpSession>();
}

// Command đăng nhập với IP
public class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string IpAddress { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; }
    public string Username { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public DateTime ExpiryTime { get; set; }
}

// Command kiểm tra hoạt động dựa trên IP
public class CheckActiveByIpCommand : IRequest<CheckActiveResponse>
{
    public string Username { get; set; }
    public string IpAddress { get; set; }
}

public class CheckActiveResponse
{
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public bool Exists { get; set; }
    public string Message { get; set; }
    public DateTime? ExpiryTime { get; set; }
}

// Handler đăng nhập với IP
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string USERS_KEY = "users";
    private readonly string _jwtSecret = "YourSecretKeyHereMakeSureItIsLongEnoughForSecurity"; // Trong thực tế, lấy từ config
    private const int SESSION_TIMEOUT_MINUTES = 5; // Thời gian hết hạn 5 phút

    public LoginCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Lấy danh sách người dùng từ state store
        var users = await _daprClient.GetStateAsync<List<UserData>>(
            STORE_NAME,
            USERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<UserData>();

        // Tìm người dùng theo username
        var user = users.FirstOrDefault(u => 
            string.Equals(u.Username, command.Username, StringComparison.OrdinalIgnoreCase));

        // Kiểm tra người dùng tồn tại và mật khẩu khớp
        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Username or password is incorrect"
            };
        }

        // Kiểm tra mật khẩu (trong thực tế, cần dùng hàm băm (hash) an toàn)
        if (!VerifyPassword(command.Password, user.PasswordHash))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Username or password is incorrect"
            };
        }

        // Kiểm tra tài khoản có active không
        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Account is inactive"
            };
        }

        // Khởi tạo danh sách IP sessions nếu chưa có
        if (user.IpSessions == null)
        {
            user.IpSessions = new List<UserIpSession>();
        }

        // Xóa các phiên IP đã hết hạn
        user.IpSessions.RemoveAll(session => !session.IsActive);

        // Tạo hoặc cập nhật phiên IP
        var existingSession = user.IpSessions.FirstOrDefault(s => s.IpAddress == command.IpAddress);
        var now = DateTime.UtcNow;
        var expiryTime = now.AddMinutes(SESSION_TIMEOUT_MINUTES);

        if (existingSession != null)
        {
            existingSession.LoginTime = now;
            existingSession.ExpiryTime = expiryTime;
        }
        else
        {
            user.IpSessions.Add(new UserIpSession
            {
                Username = user.Username,
                IpAddress = command.IpAddress,
                LoginTime = now,
                ExpiryTime = expiryTime
            });
        }

        // Lưu cập nhật người dùng vào state store
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            USERS_KEY,
            users,
            cancellationToken: cancellationToken
        );

        // Tạo token JWT
        var token = GenerateJwtToken(user, command.IpAddress, expiryTime);

        return new LoginResponse
        {
            Success = true,
            Token = token,
            Username = user.Username,
            Message = "Login successful",
            ExpiryTime = expiryTime
        };
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Trong thực tế, sử dụng hàm băm như BCrypt.Net
        // Ví dụ: return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        
        // Đây là giả lập đơn giản, cần thay thế bằng hàm băm thực tế
        return passwordHash == HashPassword(password);
    }

    private string HashPassword(string password)
    {
        // Trong thực tế, sử dụng hàm băm như BCrypt.Net
        // Ví dụ: return BCrypt.Net.BCrypt.HashPassword(password);
        
        // Đây là giả lập đơn giản, cần thay thế bằng hàm băm thực tế
        return password + "_hashed";
    }

    private string GenerateJwtToken(UserData user, string ipAddress, DateTime expiryTime)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim("IsActive", user.IsActive.ToString()),
            new Claim("IpAddress", ipAddress)
        };

        // Thêm roles vào claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: "your_issuer",
            audience: "your_audience",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiryTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Handler kiểm tra hoạt động dựa trên IP
public class CheckActiveByIpCommandHandler : IRequestHandler<CheckActiveByIpCommand, CheckActiveResponse>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string USERS_KEY = "users";

    public CheckActiveByIpCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<CheckActiveResponse> Handle(CheckActiveByIpCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        
        // Lấy danh sách người dùng từ state store
        var users = await _daprClient.GetStateAsync<List<UserData>>(
            STORE_NAME,
            USERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<UserData>();

        // Tìm người dùng theo username
        var user = users.FirstOrDefault(u => 
            string.Equals(u.Username, command.Username, StringComparison.OrdinalIgnoreCase));

        // Kiểm tra người dùng tồn tại
        if (user == null)
        {
            return new CheckActiveResponse
            {
                Exists = false,
                IsActive = false,
                Message = "User does not exist"
            };
        }

        // Kiểm tra tài khoản có active không
        if (!user.IsActive)
        {
            return new CheckActiveResponse
            {
                Exists = true,
                IsActive = false,
                Roles = user.Roles,
                Message = "User account is inactive"
            };
        }

        // Khởi tạo danh sách IP sessions nếu chưa có
        if (user.IpSessions == null)
        {
            user.IpSessions = new List<UserIpSession>();
        }

        // Tìm phiên IP
        var ipSession = user.IpSessions.FirstOrDefault(s => s.IpAddress == command.IpAddress);

        // Kiểm tra phiên IP có tồn tại và còn hiệu lực
        if (ipSession == null)
        {
            return new CheckActiveResponse
            {
                Exists = true,
                IsActive = false,
                Roles = user.Roles,
                Message = "IP session not found"
            };
        }

        if (!ipSession.IsActive)
        {
            return new CheckActiveResponse
            {
                Exists = true,
                IsActive = false,
                Roles = user.Roles,
                Message = "IP session has expired",
                ExpiryTime = ipSession.ExpiryTime
            };
        }

        // Phiên IP hợp lệ
        return new CheckActiveResponse
        {
            Exists = true,
            IsActive = true,
            Roles = user.Roles,
            Message = "User is active from this IP",
            ExpiryTime = ipSession.ExpiryTime
        };
    }
}