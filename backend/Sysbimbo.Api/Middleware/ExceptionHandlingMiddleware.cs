using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Sysbimbo.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            if (ContainsDatabaseConnectivityError(exception))
            {
                await WriteErrorAsync(
                    context,
                    HttpStatusCode.ServiceUnavailable,
                    "No fue posible conectarse a SQL Server. Revisa la cadena de conexion y la configuracion de cifrado.");
                return;
            }

            await WriteErrorAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (SqlException)
        {
            await WriteErrorAsync(
                context,
                HttpStatusCode.ServiceUnavailable,
                "No fue posible conectarse a SQL Server. Revisa la cadena de conexion y la configuracion de cifrado.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing request.");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "Ocurrio un error inesperado.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            statusCode = context.Response.StatusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }

    private static bool ContainsDatabaseConnectivityError(Exception exception)
    {
        if (exception is SqlException)
        {
            return true;
        }

        if (exception.InnerException is null)
        {
            return exception.Message.Contains("EnableRetryOnFailure", StringComparison.OrdinalIgnoreCase);
        }

        return ContainsDatabaseConnectivityError(exception.InnerException);
    }
}
