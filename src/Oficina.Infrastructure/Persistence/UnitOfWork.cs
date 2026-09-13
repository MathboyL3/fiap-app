using Oficina.Domain.Common;

namespace Oficina.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OficinaDbContext _ctx;
    public UnitOfWork(OficinaDbContext ctx) => _ctx = ctx;
    public Task<int> CommitAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);
}
