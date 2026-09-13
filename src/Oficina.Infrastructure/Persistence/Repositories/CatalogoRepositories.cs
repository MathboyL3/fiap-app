using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Catalogo;

namespace Oficina.Infrastructure.Persistence.Repositories;

public sealed class ServicoRepository : IServicoRepository
{
    private readonly OficinaDbContext _ctx;
    public ServicoRepository(OficinaDbContext ctx) => _ctx = ctx;

    public Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Servicos.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Servico>> ListarAsync(bool somenteAtivos, CancellationToken ct = default)
    {
        var q = _ctx.Servicos.AsQueryable();
        if (somenteAtivos) q = q.Where(s => s.Ativo);
        return await q.OrderBy(s => s.Nome).ToListAsync(ct);
    }

    public Task AdicionarAsync(Servico s, CancellationToken ct = default) { _ctx.Servicos.Add(s); return Task.CompletedTask; }
    public Task RemoverAsync(Servico s, CancellationToken ct = default) { _ctx.Servicos.Remove(s); return Task.CompletedTask; }
}

public sealed class PecaRepository : IPecaRepository
{
    private readonly OficinaDbContext _ctx;
    public PecaRepository(OficinaDbContext ctx) => _ctx = ctx;

    public Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Pecas.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Peca?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        _ctx.Pecas.FirstOrDefaultAsync(p => p.Codigo == codigo.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Peca>> ListarAsync(bool somenteAtivas, CancellationToken ct = default)
    {
        var q = _ctx.Pecas.AsQueryable();
        if (somenteAtivas) q = q.Where(p => p.Ativo);
        return await q.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public Task AdicionarAsync(Peca p, CancellationToken ct = default) { _ctx.Pecas.Add(p); return Task.CompletedTask; }
    public Task RemoverAsync(Peca p, CancellationToken ct = default) { _ctx.Pecas.Remove(p); return Task.CompletedTask; }
}
