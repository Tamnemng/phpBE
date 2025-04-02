using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using MediatR;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

public class IpAuthMiddleware
{
    private readonly RequestDelegate _next;

    public IpAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Nếu người dùng đã được xác thực (thông qua JWT), bỏ qua kiểm tra IP
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        // Lấy địa chỉ IP của client
        string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Lấy username từ cookie hoặc header nếu có
        string username = null;
        
        // Ưu tiên lấy từ cookie
        if (context.Request.Cookies.TryGetValue("Username", out string cookieUsername))
        {
            username = cookieUsername;
        }
        // Nếu không có cookie, thử lấy từ header
        else if (context.Request.Headers.TryGetValue("X-Username", out var headerUsername))
        {
            username = headerUsername;
        }

        // Nếu có username, kiểm tra session IP
        if (!string.IsNullOrEmpty(username))
        {
            var mediator = context.RequestServices.GetRequiredService<IMediator>();
            var command = new CheckActiveByIpCommand
            {
                Username = username,
                IpAddress = ipAddress
            };

            var result = await mediator.Send(command);

            // Nếu session IP hợp lệ, thiết lập thông tin xác thực cho người dùng
            if (result.Exists && result.IsActive)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim("IsActive", "true"),
                    new Claim("IpAddress", ipAddress),
                    new Claim("IpAuthenticated", "true")
                };

                // Thêm roles vào claims
                foreach (var role in result.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, "IpSession");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;

                // Đặt cookie session để duy trì trạng thái đăng nhập
                context.Response.Cookies.Append("Username", username, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = result.ExpiryTime
                });
            }
        }

        await _next(context);
    }
}