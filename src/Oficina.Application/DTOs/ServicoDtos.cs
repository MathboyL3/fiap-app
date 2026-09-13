namespace Oficina.Application.DTOs;

public record CadastrarServicoRequest(string Nome, string Descricao, decimal ValorBase, int TempoEstimadoMinutos);

public record AtualizarServicoRequest(string? Nome, string? Descricao, decimal? ValorBase, int? TempoEstimadoMinutos);

public record ServicoResponse(Guid Id, string Nome, string Descricao, decimal ValorBase, int TempoEstimadoMinutos, bool Ativo);
