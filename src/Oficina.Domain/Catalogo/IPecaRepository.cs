namespace Oficina.Domain.Catalogo;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Peca?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Peca>> ListarAsync(bool somenteAtivas, CancellationToken ct = default);
    Task AdicionarAsync(Peca peca, CancellationToken ct = default);
    Task RemoverAsync(Peca peca, CancellationToken ct = default);
}
