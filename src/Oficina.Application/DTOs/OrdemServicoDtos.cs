namespace Oficina.Application.DTOs;

public record AbrirOSRequest(Guid ClienteId, Guid VeiculoId);

public record IncluirItemServicoRequest(Guid ServicoId, int Quantidade);

public record IncluirItemPecaRequest(Guid PecaId, int Quantidade);

public record IniciarDiagnosticoRequest(string? Observacoes);

public record RejeitarOrcamentoRequest(string Motivo);

/// <summary>
/// Payload do webhook externo de aprovação de orçamento (Fase 2).
/// Um sistema externo notifica se o cliente aprovou ou rejeitou o orçamento da OS.
/// </summary>
public record WebhookAprovacaoRequest(Guid OrdemServicoId, bool Aprovado, string? Motivo);

public record CancelarOSRequest(string Motivo);

public record ItemServicoResponse(Guid Id, Guid ServicoId, string Descricao, decimal ValorUnitario, int Quantidade, decimal Subtotal);

public record ItemPecaResponse(Guid Id, Guid PecaId, string Descricao, decimal ValorUnitario, int Quantidade, decimal Subtotal);

public record HistoricoStatusResponse(string De, string Para, DateTime OcorreuEm, string? Observacao);

public record OSResponse(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string Status,
    decimal ValorTotal,
    DateTime CriadaEm,
    DateTime? IniciadaExecucaoEm,
    DateTime? FinalizadaEm,
    DateTime? EntregueEm,
    string? ObservacoesDiagnostico,
    IReadOnlyList<ItemServicoResponse> Servicos,
    IReadOnlyList<ItemPecaResponse> Pecas,
    IReadOnlyList<HistoricoStatusResponse> Historico);

public record OSResumoResponse(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string Status,
    decimal ValorTotal,
    DateTime CriadaEm);

public record TempoMedioExecucaoResponse(int Quantidade, double TempoMedioMinutos, double TempoMedioHoras);

public record StatusPublicoResponse(Guid Id, string Status, DateTime CriadaEm, DateTime? EstimativaConclusao);
