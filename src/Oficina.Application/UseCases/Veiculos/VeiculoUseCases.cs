using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Mapping;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Veiculos;

namespace Oficina.Application.UseCases.Veiculos;

public sealed class VeiculoUseCases
{
    private readonly IVeiculoRepository _repo;
    private readonly IClienteRepository _clientes;
    private readonly IUnitOfWork _uow;

    public VeiculoUseCases(IVeiculoRepository repo, IClienteRepository clientes, IUnitOfWork uow)
    {
        _repo = repo;
        _clientes = clientes;
        _uow = uow;
    }

    public async Task<VeiculoResponse> CadastrarAsync(CadastrarVeiculoRequest req, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(req.ClienteId, ct)
            ?? throw new NotFoundException("Cliente", req.ClienteId);

        var placa = Placa.Create(req.Placa);
        var existente = await _repo.ObterPorPlacaAsync(placa, ct);
        if (existente is not null)
            throw new ConflictException($"Já existe veículo com placa {placa}.");

        var veiculo = Veiculo.Cadastrar(cliente.Id, placa, req.Marca, req.Modelo, req.Ano);
        await _repo.AdicionarAsync(veiculo, ct);
        await _uow.CommitAsync(ct);
        return veiculo.ToResponse();
    }

    public async Task<VeiculoResponse> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Veículo", id);
        return v.ToResponse();
    }

    public async Task<IReadOnlyList<VeiculoResponse>> ListarPorClienteAsync(Guid clienteId, CancellationToken ct = default)
    {
        var list = await _repo.ListarPorClienteAsync(clienteId, ct);
        return list.Select(v => v.ToResponse()).ToList();
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var v = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Veículo", id);
        await _repo.RemoverAsync(v, ct);
        await _uow.CommitAsync(ct);
    }
}
