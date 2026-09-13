using Microsoft.Extensions.Logging;
using Oficina.Application.Notificacoes;

namespace Oficina.Infrastructure.Notificacoes;

/// <summary>
/// Implementação stub do <see cref="INotificador"/> para o MVP (Fase 2):
/// registra a notificação em log estruturado. Uma integração real (e-mail/SMS)
/// substituiria esta classe sem impactar a camada de aplicação.
/// </summary>
public sealed class LogNotificador : INotificador
{
    private readonly ILogger<LogNotificador> _logger;

    public LogNotificador(ILogger<LogNotificador> logger) => _logger = logger;

    public Task NotificarMudancaStatusAsync(Guid ordemServicoId, string novoStatus, string mensagem, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[NOTIFICAÇÃO] OS {OrdemServicoId} mudou para status '{NovoStatus}': {Mensagem}",
            ordemServicoId, novoStatus, mensagem);
        return Task.CompletedTask;
    }
}
