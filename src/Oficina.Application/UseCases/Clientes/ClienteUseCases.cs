using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Mapping;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.UseCases.Clientes;

public sealed class ClienteUseCases
{
    private readonly IClienteRepository _repo;
    private readonly IUnitOfWork _uow;

    public ClienteUseCases(IClienteRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest req, CancellationToken ct = default)
    {
        var doc = Documento.Parse(req.Documento);
        var existente = await _repo.ObterPorDocumentoAsync(doc, ct);
        if (existente is not null)
            throw new ConflictException($"Já existe cliente com documento {doc}.");

        var cliente = Cliente.Cadastrar(req.Nome, doc, req.Email, req.Telefone);
        await _repo.AdicionarAsync(cliente, ct);
        await _uow.CommitAsync(ct);
        return cliente.ToResponse();
    }

    public async Task<ClienteResponse> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Cliente", id);
        return c.ToResponse();
    }

    public async Task<ClienteResponse?> BuscarPorDocumentoAsync(string documento, CancellationToken ct = default)
    {
        var doc = Documento.Parse(documento);
        var c = await _repo.ObterPorDocumentoAsync(doc, ct);
        return c?.ToResponse();
    }

    public async Task<IReadOnlyList<ClienteResponse>> ListarAsync(int skip, int take, CancellationToken ct = default)
    {
        var list = await _repo.ListarAsync(skip, take, ct);
        return list.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ClienteResponse> AtualizarAsync(Guid id, AtualizarClienteRequest req, CancellationToken ct = default)
    {
        var c = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Cliente", id);
        c.Atualizar(req.Nome ?? "", req.Email ?? "", req.Telefone ?? "");
        await _uow.CommitAsync(ct);
        return c.ToResponse();
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Cliente", id);
        await _repo.RemoverAsync(c, ct);
        await _uow.CommitAsync(ct);
    }
}
