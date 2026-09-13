using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Clientes;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Cliente?> ObterPorDocumentoAsync(Documento documento, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(int skip, int take, CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);
    Task RemoverAsync(Cliente cliente, CancellationToken ct = default);
    Task<int> ContarAsync(CancellationToken ct = default);
}
