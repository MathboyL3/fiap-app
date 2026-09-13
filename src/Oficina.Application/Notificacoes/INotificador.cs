namespace Oficina.Application.Notificacoes;

/// <summary>
/// Porta de notificação ao cliente (Fase 2). A implementação concreta (e-mail, SMS, etc.)
/// vive na camada de Infraestrutura. No MVP usamos um stub que apenas registra em log.
/// </summary>
public interface INotificador
{
    /// <summary>
    /// Notifica o cliente sobre a mudança de status de uma Ordem de Serviço.
    /// </summary>
    Task NotificarMudancaStatusAsync(Guid ordemServicoId, string novoStatus, string mensagem, CancellationToken ct = default);
}
