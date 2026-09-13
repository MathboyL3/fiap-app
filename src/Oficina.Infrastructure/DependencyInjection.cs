using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.Auth;
using Oficina.Application.Notificacoes;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.Estoque;
using Oficina.Domain.Identidade;
using Oficina.Domain.OrdensServico;
using Oficina.Domain.Veiculos;
using Oficina.Infrastructure.Auth;
using Oficina.Infrastructure.Notificacoes;
using Oficina.Infrastructure.Persistence;
using Oficina.Infrastructure.Persistence.Repositories;

namespace Oficina.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        services.AddScoped<IPecaRepository, PecaRepository>();
        services.AddScoped<IEstoquePecaRepository, EstoquePecaRepository>();
        services.AddScoped<IOrdemDeServicoRepository, OrdemDeServicoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<INotificador, LogNotificador>();

        return services;
    }
}
