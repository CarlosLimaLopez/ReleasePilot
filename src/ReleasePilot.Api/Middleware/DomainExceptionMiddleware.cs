using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ReleasePilot.Domain.Exceptions;

namespace ReleasePilot.Api.Middleware;

public class DomainExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Domain rule violation",
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails));
        }
    }
}