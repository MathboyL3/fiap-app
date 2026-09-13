using Microsoft.EntityFrameworkCore;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Veiculos;

namespace Oficina.Infrastructure.Persistence.Repositories;

public sealed class VeiculoRepository : IVeiculoRepository
{
    private readonly OficinaDbContext _ctx;
    public VeiculoRepository(OficinaDbContext ctx) => _ctx = ctx;

    public Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Veiculos.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken ct = default) =>
        _ctx.Veiculos.FirstOrDefaultAsync(v => v.Placa.Valor == placa.Valor, ct);

    public async Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct = default) =>
        await _ctx.Veiculos.Where(v => v.ClienteId == clienteId).OrderBy(v => v.Marca).ToListAsync(ct);

    public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default) { _ctx.Veiculos.Add(veiculo); return Task.CompletedTask; }
    public Task RemoverAsync(Veiculo veiculo, CancellationToken ct = default) { _ctx.Veiculos.Remove(veiculo); return Task.CompletedTask; }
}
