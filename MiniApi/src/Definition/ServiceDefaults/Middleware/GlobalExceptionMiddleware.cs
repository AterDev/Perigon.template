using Npgsql;
using Microsoft.Extensions.Logging;
using Share.Exceptions;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    Localizer localizer,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex) when (ctx.Response.HasStarted ||
            (ex is OperationCanceledException && ctx.RequestAborted.IsCancellationRequested))
        {
            throw;
        }
        catch (PostgresException ex) when (PostgreSqlErrorHelper.IsUniqueConstraintViolation(ex))
        {
            // 唯一约束冲突提示
            await TypedResults.Problem(detail: localizer.Get(Localizer.ConflictResource),
                statusCode: StatusCodes.Status409Conflict).ExecuteAsync(ctx);
        }
        catch (PostgresException ex)
        {
            logger.LogError(ex, "Database request failed. RequestId: {RequestId}", ctx.TraceIdentifier);
            await TypedResults.Problem(detail: environment.IsDevelopment() ? ex.Message : localizer.Get(Localizer.InternalServerError),
                statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(ctx);
        }
        catch (BusinessException ex)
        {
            await TypedResults.Problem(detail: localizer.Get(ex.LanguageKey),
                statusCode: ex.StatusCodes).ExecuteAsync(ctx);
        }
        catch (BadHttpRequestException ex)
        {
            await TypedResults.Problem(detail: ex.Message, statusCode: ex.StatusCode).ExecuteAsync(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled request failure. RequestId: {RequestId}", ctx.TraceIdentifier);
            await TypedResults.Problem(detail: environment.IsDevelopment() ? ex.Message : localizer.Get(Localizer.InternalServerError),
                statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(ctx);
        }
    }
}

public static class PostgreSqlErrorHelper
{
    public static bool IsUniqueConstraintViolation(PostgresException ex) => ex.SqlState == "23505";
}
