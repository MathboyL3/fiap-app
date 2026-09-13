namespace Oficina.Domain.Common;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
