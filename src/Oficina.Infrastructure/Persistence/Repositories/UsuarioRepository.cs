using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Identidade;

namespace Oficina.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly OficinaDbContext _ctx;
    public UsuarioRepository(OficinaDbContext ctx) => _ctx = ctx;

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        _ctx.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
    {
        _ctx.Usuarios.Add(usuario);
        return Task.CompletedTask;
    }
}
