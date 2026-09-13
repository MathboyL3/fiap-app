using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Veiculos;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken ct = default);
    Task<IReadOnlyList<Veiculo>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken ct = default);
    Task RemoverAsync(Veiculo veiculo, CancellationToken ct = default);
}
