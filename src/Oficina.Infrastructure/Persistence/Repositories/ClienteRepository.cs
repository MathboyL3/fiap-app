using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Clientes;
using Oficina.Domain.ValueObjects;

namespace Oficina.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly OficinaDbContext _ctx;
    public ClienteRepository(OficinaDbContext ctx) => _ctx = ctx;

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> ObterPorDocumentoAsync(Documento documento, CancellationToken ct = default) =>
        _ctx.Clientes.FirstOrDefaultAsync(c => c.Documento.Numero == documento.Numero, ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(int skip, int take, CancellationToken ct = default) =>
        await _ctx.Clientes.OrderBy(c => c.Nome).Skip(skip).Take(take).ToListAsync(ct);

    public Task AdicionarAsync(Cliente cliente, CancellationToken ct = default)
    {
        _ctx.Clientes.Add(cliente);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(Cliente cliente, CancellationToken ct = default)
    {
        _ctx.Clientes.Remove(cliente);
        return Task.CompletedTask;
    }

    public Task<int> ContarAsync(CancellationToken ct = default) => _ctx.Clientes.CountAsync(ct);
}
