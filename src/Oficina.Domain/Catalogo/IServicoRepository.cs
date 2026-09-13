namespace Oficina.Domain.Catalogo;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Servico>> ListarAsync(bool somenteAtivos, CancellationToken ct = default);
    Task AdicionarAsync(Servico servico, CancellationToken ct = default);
    Task RemoverAsync(Servico servico, CancellationToken ct = default);
}
