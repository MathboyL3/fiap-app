using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Oficina.Infrastructure.Persistence;

/// <summary>
/// Permite ao EF Core CLI criar o DbContext em design-time (migrations) sem precisar
/// de Postgres rodando ou de configuração externa.
/// </summary>
public class OficinaDbContextFactory : IDesignTimeDbContextFactory<OficinaDbContext>
{
    public OficinaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseNpgsql("Host=localhost;Database=oficina_design;Username=design;Password=design")
            .Options;
        return new OficinaDbContext(options);
    }
}
