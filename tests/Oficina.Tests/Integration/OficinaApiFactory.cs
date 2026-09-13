using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Oficina.Infrastructure.Persistence;

namespace Oficina.Tests.Integration;

public sealed class OficinaApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"oficina-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<OficinaDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
