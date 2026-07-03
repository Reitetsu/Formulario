using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Sysbimbo.Api.Data;

namespace Sysbimbo.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(SysbimboDbContext dbContext) : ControllerBase
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
                    message = "No fue posible conectarse a SQL Server."
                });
            }

            var databaseName = dbContext.Database.GetDbConnection().Database;

            return Ok(new
            {
                success = true,
                message = "Conexion a SQL Server exitosa.",
                database = databaseName
            });
        }
        catch (Exception exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                success = false,
                message = "La conexion a SQL Server fallo.",
                detail = exception.Message
            });
        }
    }
}
