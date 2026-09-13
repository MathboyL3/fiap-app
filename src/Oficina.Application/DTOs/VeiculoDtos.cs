namespace Oficina.Application.DTOs;

public record CadastrarVeiculoRequest(Guid ClienteId, string Placa, string Marca, string Modelo, int Ano);

public record VeiculoResponse(Guid Id, Guid ClienteId, string Placa, string Marca, string Modelo, int Ano);
