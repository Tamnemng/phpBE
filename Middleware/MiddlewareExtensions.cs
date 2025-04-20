using Microsoft.AspNetCore.Builder;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseActiveSessionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ActiveSessionMiddleware>();
    }
}