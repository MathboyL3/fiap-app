using Microsoft.EntityFrameworkCore;
using Oficina.Domain.OrdensServico;

namespace Oficina.Infrastructure.Persistence.Repositories;

public sealed class OrdemDeServicoRepository : IOrdemDeServicoRepository
{
    private readonly OficinaDbContext _ctx;
    public OrdemDeServicoRepository(OficinaDbContext ctx) => _ctx = ctx;

    public Task<OrdemDeServico?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.OrdensServico
            .Include(o => o.Servicos)
            .Include(o => o.Pecas)
            .Include(o => o.Historico)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<OrdemDeServico>> ListarAsync(int skip, int take, StatusOS? status, CancellationToken ct = default)
    {
        var q = _ctx.OrdensServico.AsQueryable();
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        return await q.OrderByDescending(o => o.CriadaEm).Skip(skip).Take(take).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrdemDeServico>> ListarPainelAsync(int skip, int take, CancellationToken ct = default)
    {
        // Exclui OS já concluídas/canceladas do painel de acompanhamento.
        var ativos = new[] { StatusOS.EmExecucao, StatusOS.AguardandoAprovacao, StatusOS.EmDiagnostico, StatusOS.Recebida };
        return await _ctx.OrdensServico
            .Where(o => ativos.Contains(o.Status))
            // Prioridade: EmExecucao(4) > AguardandoAprovacao(3) > EmDiagnostico(2) > Recebida(1).
            // Menor "peso" = maior prioridade, então ordenamos ascendente pelo peso.
            .OrderBy(o =>
                o.Status == StatusOS.EmExecucao ? 0 :
                o.Status == StatusOS.AguardandoAprovacao ? 1 :
                o.Status == StatusOS.EmDiagnostico ? 2 : 3)
            // Dentro do mesmo status, as mais antigas primeiro.
            .ThenBy(o => o.CriadaEm)
            .Skip(skip).Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrdemDeServico>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct = default) =>
        await _ctx.OrdensServico.Where(o => o.ClienteId == clienteId).OrderByDescending(o => o.CriadaEm).ToListAsync(ct);

    public async Task<IReadOnlyList<OrdemDeServico>> ListarFinalizadasNoIntervaloAsync(DateTime de, DateTime ate, CancellationToken ct = default) =>
        await _ctx.OrdensServico
            .Where(o => o.FinalizadaEm != null && o.FinalizadaEm >= de && o.FinalizadaEm <= ate)
            .ToListAsync(ct);

    public Task AdicionarAsync(OrdemDeServico os, CancellationToken ct = default)
    {
        _ctx.OrdensServico.Add(os);
        return Task.CompletedTask;
    }

    public Task<int> ContarAsync(StatusOS? status, CancellationToken ct = default)
    {
        var q = _ctx.OrdensServico.AsQueryable();
        if (status.HasValue) q = q.Where(o => o.Status == status.Value);
        return q.CountAsync(ct);
    }
}
