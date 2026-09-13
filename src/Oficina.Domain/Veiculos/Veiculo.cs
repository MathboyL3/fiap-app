using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Veiculos;

public sealed class Veiculo : Entity
{
    public Guid ClienteId { get; private set; }
    public Placa Placa { get; private set; }
    public string Marca { get; private set; }
    public string Modelo { get; private set; }
    public int Ano { get; private set; }

    private Veiculo() { Placa = null!; Marca = Modelo = null!; }

    public static Veiculo Cadastrar(Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        if (clienteId == Guid.Empty)
            throw new DomainException("Veículo deve estar associado a um cliente.");
        if (string.IsNullOrWhiteSpace(marca))
            throw new DomainException("Marca do veículo é obrigatória.");
        if (string.IsNullOrWhiteSpace(modelo))
            throw new DomainException("Modelo do veículo é obrigatório.");
        var anoAtual = DateTime.UtcNow.Year;
        if (ano < 1900 || ano > anoAtual + 1)
            throw new DomainException($"Ano do veículo deve estar entre 1900 e {anoAtual + 1}.");

        return new Veiculo
        {
            ClienteId = clienteId,
            Placa = placa,
            Marca = marca.Trim(),
            Modelo = modelo.Trim(),
            Ano = ano
        };
    }
}
