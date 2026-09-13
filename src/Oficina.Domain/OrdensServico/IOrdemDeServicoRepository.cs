namespace Oficina.Domain.OrdensServico;

public interface IOrdemDeServicoRepository
{
    Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OrdemDeServico>> ListarAsync(int skip, int take, StatusOS? status, CancellationToken ct = default);

    /// <summary>
    /// Painel de OS ativas (Fase 2): exclui Finalizada/Entregue/Cancelada,
    /// ordena por prioridade de status (EmExecucao &gt; AguardandoAprovacao &gt; EmDiagnostico &gt; Recebida)
    /// e, dentro do mesmo status, das mais antigas para as mais recentes (CriadaEm ascendente).
    /// </summary>
    Task<IReadOnlyList<OrdemDeServico>> ListarPainelAsync(int skip, int take, CancellationToken ct = default);
    Task<IReadOnlyList<OrdemDeServico>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<IReadOnlyList<OrdemDeServico>> ListarFinalizadasNoIntervaloAsync(DateTime de, DateTime ate, CancellationToken ct = default);
    Task AdicionarAsync(OrdemDeServico os, CancellationToken ct = default);
    Task<int> ContarAsync(StatusOS? status, CancellationToken ct = default);
}
