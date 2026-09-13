namespace Oficina.Application.DTOs;

public record CadastrarPecaRequest(string Nome, string Codigo, string Unidade, decimal Valor, int QuantidadeInicial, int LimiteMinimo);

public record AtualizarValorPecaRequest(decimal NovoValor);

public record ReabastecerEstoqueRequest(int Quantidade, string? Referencia);

public record PecaResponse(
    Guid Id,
    string Nome,
    string Codigo,
    string Unidade,
    decimal Valor,
    bool Ativo,
    int QuantidadeDisponivel,
    int QuantidadeReservada,
    int LimiteMinimo,
    bool EstoqueBaixo);
