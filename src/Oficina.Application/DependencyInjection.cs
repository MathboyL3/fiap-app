using Microsoft.Extensions.DependencyInjection;
using Oficina.Application.UseCases.Auth;
using Oficina.Application.UseCases.Clientes;
using Oficina.Application.UseCases.OrdensServico;
using Oficina.Application.UseCases.Pecas;
using Oficina.Application.UseCases.Servicos;
using Oficina.Application.UseCases.Veiculos;

namespace Oficina.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ClienteUseCases>();
        services.AddScoped<VeiculoUseCases>();
        services.AddScoped<ServicoUseCases>();
        services.AddScoped<PecaUseCases>();
        services.AddScoped<OrdemServicoUseCases>();
        services.AddScoped<AuthUseCases>();
        return services;
    }
}
