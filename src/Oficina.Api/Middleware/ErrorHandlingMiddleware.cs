using System.Text.Json;
using Oficina.Application.Common;
using Oficina.Domain.Common;

namespace Oficina.Api.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _log;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (NotFoundException ex)
        {
            await Write(ctx, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await Write(ctx, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DomainException ex)
        {
            await Write(ctx, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (AppException ex)
        {
            await Write(ctx, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Erro não tratado");
            await Write(ctx, StatusCodes.Status500InternalServerError, "Erro interno do servidor.");
        }
    }

    private static async Task Write(HttpContext ctx, int status, string message)
    {
        ctx.Response.Clear();
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { status, message });
        await ctx.Response.WriteAsync(body);
    }
}
