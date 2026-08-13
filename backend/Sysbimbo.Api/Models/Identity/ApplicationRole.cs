using Microsoft.AspNetCore.Identity;

namespace Sysbimbo.Api.Models.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
