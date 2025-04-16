using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);

            // If we reach this point and there's a 400 status code but no response body yet, 
            // it's likely a validation error from model binding
            if (context.Response.StatusCode == 400 && !context.Response.HasStarted && context.Response.ContentLength == null)
            {
                var response = ApiResponse<object>.CreateError(
                    "One or more validation errors occurred.",
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR"
                );

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "PRODUCT_INVALID");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "SERVER_ERROR");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string errorCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.CreateError(
            exception.Message,
            statusCode,
            errorCode
        );

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}

// Extension method to make it easier to add the middleware
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}