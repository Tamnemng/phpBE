// Middleware/ActiveSessionMiddleware.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Think4.Services;
using System.Security.Claims;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

public class ActiveSessionMiddleware
{
    private readonly RequestDelegate _next;
    
    public ActiveSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip middleware for authentication endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth/login") ||
            context.Request.Path.StartsWithSegments("/api/auth/register") ||
            context.Request.Path.StartsWithSegments("/api/auth/check-session") ||
            !context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }
        
        // Get user ID from claims
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            await HandleUnauthorized(context, "Unauthorized", "AUTH_ERROR");
            return;
        }
        
        // Get the auth service from the request scope
        var authService = context.RequestServices.GetRequiredService<IAuthService>();
        
        // Check if session is active
        var isActive = await authService.IsSessionActive(userId);
        if (!isActive)
        {
            await HandleUnauthorized(context, "Session expired", "SESSION_EXPIRED");
            return;
        }
        
        // Update last active timestamp
        await authService.UpdateUserLastActive(userId);
        
        // Continue with the request
        await _next(context);
    }
    
    private async Task HandleUnauthorized(HttpContext context, string message, string errorCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        
        var response = ApiResponse<object>.CreateError(
            message,
            HttpStatusCode.Unauthorized,
            errorCode
        );
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}