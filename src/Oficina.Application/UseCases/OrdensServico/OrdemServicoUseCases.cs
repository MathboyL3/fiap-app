using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Mapping;
using Oficina.Application.Notificacoes;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.Estoque;
using Oficina.Domain.OrdensServico;
using Oficina.Domain.Veiculos;

namespace Oficina.Application.UseCases.OrdensServico;

public sealed class OrdemServicoUseCases
{
    private readonly IOrdemDeServicoRepository _repo;
    private readonly IClienteRepository _clientes;
    private readonly IVeiculoRepository _veiculos;
    private readonly IServicoRepository _servicos;
    private readonly IPecaRepository _pecas;
    private readonly IEstoquePecaRepository _estoque;
    private readonly IUnitOfWork _uow;
    private readonly INotificador _notificador;

    public OrdemServicoUseCases(
        IOrdemDeServicoRepository repo,
        IClienteRepository clientes,
        IVeiculoRepository veiculos,
        IServicoRepository servicos,
        IPecaRepository pecas,
        IEstoquePecaRepository estoque,
        IUnitOfWork uow,
        INotificador notificador)
    {
        _repo = repo;
        _clientes = clientes;
        _veiculos = veiculos;
        _servicos = servicos;
        _pecas = pecas;
        _estoque = estoque;
        _uow = uow;
        _notificador = notificador;
    }

    public async Task<OSResponse> AbrirAsync(AbrirOSRequest req, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(req.ClienteId, ct)
            ?? throw new NotFoundException("Cliente", req.ClienteId);
        var veiculo = await _veiculos.ObterPorIdAsync(req.VeiculoId, ct)
            ?? throw new NotFoundException("Veículo", req.VeiculoId);
        if (veiculo.ClienteId != cliente.Id)
            throw new ConflictException("Veículo não pertence ao cliente informado.");

        var os = OrdemDeServico.Abrir(cliente.Id, veiculo.Id);
        await _repo.AdicionarAsync(os, ct);
        await _uow.CommitAsync(ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Ordem de Serviço", id);
        return os.ToResponse();
    }

    public async Task<IReadOnlyList<OSResumoResponse>> ListarAsync(int skip, int take, StatusOS? status, CancellationToken ct = default)
    {
        var list = await _repo.ListarAsync(skip, take, status, ct);
        return list.Select(o => o.ToResumo()).ToList();
    }

    /// <summary>
    /// Painel de acompanhamento (Fase 2): OS ativas ordenadas por prioridade de status
    /// (Em execução &gt; Aguardando aprovação &gt; Em diagnóstico &gt; Recebida) e, no mesmo status,
    /// das mais antigas para as mais recentes. Finalizadas/Entregues/Canceladas ficam de fora.
    /// </summary>
    public async Task<IReadOnlyList<OSResumoResponse>> ListarPainelAsync(int skip, int take, CancellationToken ct = default)
    {
        var list = await _repo.ListarPainelAsync(skip, take, ct);
        return list.Select(o => o.ToResumo()).ToList();
    }

    public async Task<OSResponse> IncluirServicoAsync(Guid osId, IncluirItemServicoRequest req, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        var servico = await _servicos.ObterPorIdAsync(req.ServicoId, ct) ?? throw new NotFoundException("Serviço", req.ServicoId);
        if (!servico.Ativo) throw new ConflictException("Serviço inativo não pode ser incluído na OS.");
        os.IncluirServico(servico.Id, servico.Nome, servico.ValorBase, req.Quantidade);
        await _uow.CommitAsync(ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> IncluirPecaAsync(Guid osId, IncluirItemPecaRequest req, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        var peca = await _pecas.ObterPorIdAsync(req.PecaId, ct) ?? throw new NotFoundException("Peça", req.PecaId);
        if (!peca.Ativo) throw new ConflictException("Peça inativa não pode ser incluída na OS.");
        var estoque = await _estoque.ObterPorPecaIdAsync(peca.Id, ct)
            ?? throw new NotFoundException("Estoque da peça", peca.Id);
        estoque.Reservar(req.Quantidade, $"OS:{os.Id}");
        os.IncluirPeca(peca.Id, peca.Nome, peca.Valor, req.Quantidade);
        await _uow.CommitAsync(ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> RemoverPecaAsync(Guid osId, Guid itemPecaId, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        var item = os.Pecas.SingleOrDefault(p => p.Id == itemPecaId)
            ?? throw new NotFoundException("Item de peça", itemPecaId);
        var estoque = await _estoque.ObterPorPecaIdAsync(item.PecaId, ct)
            ?? throw new NotFoundException("Estoque da peça", item.PecaId);
        estoque.EstornarReserva(item.Quantidade, $"OS:{os.Id}");
        os.RemoverPeca(itemPecaId);
        await _uow.CommitAsync(ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> IniciarDiagnosticoAsync(Guid osId, IniciarDiagnosticoRequest req, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        os.IniciarDiagnostico(req.Observacoes);
        await _uow.CommitAsync(ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> EnviarParaAprovacaoAsync(Guid osId, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        os.EnviarParaAprovacao();
        await _uow.CommitAsync(ct);
        await _notificador.NotificarMudancaStatusAsync(os.Id, os.Status.ToString(),
            "Orçamento enviado para sua aprovação.", ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> AprovarOrcamentoAsync(Guid osId, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        os.AprovarOrcamento();
        await _uow.CommitAsync(ct);
        await _notificador.NotificarMudancaStatusAsync(os.Id, os.Status.ToString(),
            "Orçamento aprovado. Serviço em execução.", ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> RejeitarOrcamentoAsync(Guid osId, RejeitarOrcamentoRequest req, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        // Estornar reservas das peças
        foreach (var item in os.Pecas)
        {
            var e = await _estoque.ObterPorPecaIdAsync(item.PecaId, ct);
            e?.EstornarReserva(item.Quantidade, $"OS rejeitada:{os.Id}");
        }
        os.RejeitarOrcamento(req.Motivo);
        await _uow.CommitAsync(ct);
        await _notificador.NotificarMudancaStatusAsync(os.Id, os.Status.ToString(),
            $"Orçamento rejeitado: {req.Motivo}", ct);
        return os.ToResponse();
    }

    /// <summary>
    /// Processa a notificação externa (webhook) de aprovação/rejeição do orçamento (Fase 2).
    /// Reaproveita os fluxos de aprovar/rejeitar, garantindo estorno de reservas na rejeição.
    /// </summary>
    public async Task<OSResponse> ProcessarWebhookAprovacaoAsync(WebhookAprovacaoRequest req, CancellationToken ct = default)
    {
        if (req.Aprovado)
            return await AprovarOrcamentoAsync(req.OrdemServicoId, ct);

        var motivo = string.IsNullOrWhiteSpace(req.Motivo) ? "Orçamento rejeitado pelo cliente." : req.Motivo!;
        return await RejeitarOrcamentoAsync(req.OrdemServicoId, new RejeitarOrcamentoRequest(motivo), ct);
    }

    public async Task<OSResponse> FinalizarAsync(Guid osId, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        // Consumir reservas
        foreach (var item in os.Pecas)
        {
            var e = await _estoque.ObterPorPecaIdAsync(item.PecaId, ct);
            e?.ConsumirReservado(item.Quantidade, $"OS finalizada:{os.Id}");
        }
        os.Finalizar();
        await _uow.CommitAsync(ct);
        await _notificador.NotificarMudancaStatusAsync(os.Id, os.Status.ToString(),
            "Serviço finalizado. Seu veículo está pronto para retirada.", ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> EntregarAsync(Guid osId, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        os.Entregar();
        await _uow.CommitAsync(ct);
        await _notificador.NotificarMudancaStatusAsync(os.Id, os.Status.ToString(),
            "Veículo entregue. Obrigado pela preferência!", ct);
        return os.ToResponse();
    }

    public async Task<OSResponse> CancelarAsync(Guid osId, CancelarOSRequest req, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(osId, ct) ?? throw new NotFoundException("Ordem de Serviço", osId);
        // Estornar reservas se houver
        foreach (var item in os.Pecas)
        {
            var e = await _estoque.ObterPorPecaIdAsync(item.PecaId, ct);
            try { e?.EstornarReserva(item.Quantidade, $"OS cancelada:{os.Id}"); } catch { /* já consumida */ }
        }
        os.Cancelar(req.Motivo);
        await _uow.CommitAsync(ct);
        return os.ToResponse();
    }

    public async Task<TempoMedioExecucaoResponse> CalcularTempoMedioAsync(DateTime? de, DateTime? ate, CancellationToken ct = default)
    {
        var inicio = ToUtc(de) ?? DateTime.UtcNow.AddDays(-30);
        var fim = ToUtc(ate) ?? DateTime.UtcNow;
        var lista = await _repo.ListarFinalizadasNoIntervaloAsync(inicio, fim, ct);
        var tempos = lista
            .Where(o => o.TempoExecucao.HasValue)
            .Select(o => o.TempoExecucao!.Value.TotalMinutes)
            .ToList();
        if (tempos.Count == 0)
            return new TempoMedioExecucaoResponse(0, 0, 0);
        var media = tempos.Average();
        return new TempoMedioExecucaoResponse(tempos.Count, Math.Round(media, 2), Math.Round(media / 60, 2));
    }

    public async Task<StatusPublicoResponse> ObterStatusPublicoAsync(Guid id, CancellationToken ct = default)
    {
        var os = await _repo.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Ordem de Serviço", id);
        DateTime? estimativa = null;
        if (os.Status == StatusOS.EmExecucao)
        {
            // Estimativa simples: soma do tempo dos serviços
            var totalMin = os.Servicos.Sum(s =>
            {
                var serv = _servicos.ObterPorIdAsync(s.ServicoId, ct).GetAwaiter().GetResult();
                return serv is null ? 0 : serv.TempoEstimado.TotalMinutes * s.Quantidade;
            });
            estimativa = os.IniciadaExecucaoEm?.AddMinutes(totalMin);
        }
        return new StatusPublicoResponse(os.Id, os.Status.ToString(), os.CriadaEm, estimativa);
    }

    private static DateTime? ToUtc(DateTime? value) => value is null ? null : value.Value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.Value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc),
    };
}
