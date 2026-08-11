using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.Data;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(FormularioDbContext dbContext) : ControllerBase
{
    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    success = false,
                    message = "No fue posible conectarse a PostgreSQL."
                });
            }

            var databaseName = dbContext.Database.GetDbConnection().Database;

            return Ok(new
            {
                success = true,
                message = "Conexion a PostgreSQL exitosa.",
                provider = "PostgreSQL",
                database = databaseName
            });
        }
        catch (Exception exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                success = false,
                message = "La conexion a PostgreSQL fallo.",
                detail = exception.Message
            });
        }
    }
}
