using Npgsql;
using Perigon.AspNetCore.Models;
using Share.Exceptions;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, Localizer localizer)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (PostgresException ex) when (PostgreSqlErrorHelper.IsUniqueConstraintViolation(ex))
        {
            // 唯一约束冲突提示
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;

            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(Localizer.ConflictResource), ctx.TraceIdentifier, status: StatusCodes.Status409Conflict)
            );
        }
        catch (PostgresException ex)
        {
            // 其他数据库错误
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(ex.Message, ctx.TraceIdentifier, "database error!")
            );
        }
        catch (BusinessException ex)
        {
            ctx.Response.StatusCode = ex.StatusCodes;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(ex.LanguageKey), ctx.TraceIdentifier, status: ex.StatusCodes)
            );
        }
        catch (BadHttpRequestException ex)
        {
            ctx.Response.StatusCode = ex.StatusCode;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(ex.Message, ctx.TraceIdentifier, status: ex.StatusCode)
            );
        }
        catch (Exception ex)
        {
            // 非数据库类异常
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier));
        }
    }
}

public static class PostgreSqlErrorHelper
{
    public static bool IsUniqueConstraintViolation(PostgresException ex) => ex.SqlState == "23505";
}
