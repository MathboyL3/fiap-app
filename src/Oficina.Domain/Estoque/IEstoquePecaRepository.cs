namespace Oficina.Domain.Estoque;

public interface IEstoquePecaRepository
{
    Task<EstoquePeca?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<EstoquePeca?> ObterPorPecaIdAsync(Guid pecaId, CancellationToken ct = default);
    Task<IReadOnlyList<EstoquePeca>> ListarBaixosAsync(CancellationToken ct = default);
    Task AdicionarAsync(EstoquePeca estoque, CancellationToken ct = default);
}
