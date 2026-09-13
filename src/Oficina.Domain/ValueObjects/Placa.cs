using System.Text.RegularExpressions;
using Oficina.Domain.Common;

namespace Oficina.Domain.ValueObjects;

public sealed partial class Placa : ValueObject
{
    public string Valor { get; }

    private Placa(string valor) => Valor = valor;

    public static Placa Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Placa não pode ser vazia.");

        var normalizada = input.Trim().ToUpperInvariant().Replace("-", "");

        if (!FormatoMercosul().IsMatch(normalizada) && !FormatoAntigo().IsMatch(normalizada))
            throw new DomainException("Placa inválida. Use o formato AAA0A00 (Mercosul) ou AAA0000 (antigo).");

        return new Placa(normalizada);
    }

    public string Formatada() => $"{Valor[..3]}-{Valor[3..]}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Formatada();

    [GeneratedRegex(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$")]
    private static partial Regex FormatoMercosul();

    [GeneratedRegex(@"^[A-Z]{3}[0-9]{4}$")]
    private static partial Regex FormatoAntigo();
}
