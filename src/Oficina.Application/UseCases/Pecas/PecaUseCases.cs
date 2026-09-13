using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Mapping;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Common;
using Oficina.Domain.Estoque;
using Oficina.Domain.ValueObjects;

namespace Oficina.Application.UseCases.Pecas;

public sealed class PecaUseCases
{
    private readonly IPecaRepository _repo;
    private readonly IEstoquePecaRepository _estoque;
    private readonly IUnitOfWork _uow;

    public PecaUseCases(IPecaRepository repo, IEstoquePecaRepository estoque, IUnitOfWork uow)
    {
        _repo = repo;
        _estoque = estoque;
        _uow = uow;
    }

    public async Task<PecaResponse> CadastrarAsync(CadastrarPecaRequest req, CancellationToken ct = default)
    {
        var existente = await _repo.ObterPorCodigoAsync(req.Codigo, ct);
        if (existente is not null)
            throw new ConflictException($"Já existe peça com código {req.Codigo}.");

        var peca = Peca.Cadastrar(req.Nome, req.Codigo, req.Unidade, Money.Brl(req.Valor));
        await _repo.AdicionarAsync(peca, ct);

        var estoque = EstoquePeca.Criar(peca.Id, req.QuantidadeInicial, req.LimiteMinimo);
        await _estoque.AdicionarAsync(estoque, ct);

        await _uow.CommitAsync(ct);
        return peca.ToResponse(estoque);
    }

    public async Task<PecaResponse> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Peça", id);
        var e = await _estoque.ObterPorPecaIdAsync(id, ct);
        return p.ToResponse(e);
    }

    public async Task<IReadOnlyList<PecaResponse>> ListarAsync(bool somenteAtivas, CancellationToken ct = default)
    {
        var pecas = await _repo.ListarAsync(somenteAtivas, ct);
        var result = new List<PecaResponse>();
        foreach (var p in pecas)
        {
            var e = await _estoque.ObterPorPecaIdAsync(p.Id, ct);
            result.Add(p.ToResponse(e));
        }
        return result;
    }

    public async Task<PecaResponse> AtualizarValorAsync(Guid id, AtualizarValorPecaRequest req, CancellationToken ct = default)
    {
        var p = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Peça", id);
        p.AtualizarValor(Money.Brl(req.NovoValor));
        await _uow.CommitAsync(ct);
        var e = await _estoque.ObterPorPecaIdAsync(id, ct);
        return p.ToResponse(e);
    }

    public async Task<PecaResponse> ReabastecerAsync(Guid pecaId, ReabastecerEstoqueRequest req, CancellationToken ct = default)
    {
        var p = await _repo.ObterPorIdAsync(pecaId, ct) ?? throw new NotFoundException("Peça", pecaId);
        var estoque = await _estoque.ObterPorPecaIdAsync(pecaId, ct)
            ?? throw new NotFoundException("Estoque da peça", pecaId);
        estoque.Reabastecer(req.Quantidade, req.Referencia);
        await _uow.CommitAsync(ct);
        return p.ToResponse(estoque);
    }

    public async Task InativarAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Peça", id);
        p.Inativar();
        await _uow.CommitAsync(ct);
    }
}
