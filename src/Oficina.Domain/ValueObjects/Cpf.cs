using System.Text.RegularExpressions;
using Oficina.Domain.Common;

namespace Oficina.Domain.ValueObjects;

public sealed partial class Cpf : ValueObject
{
    public string Numero { get; }

    private Cpf(string numero) => Numero = numero;

    public static Cpf Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("CPF não pode ser vazio.");

        var digits = NaoDigitos().Replace(input, "");

        if (digits.Length != 11)
            throw new DomainException("CPF deve conter 11 dígitos.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CPF inválido (todos os dígitos iguais).");

        if (!ValidarDigitos(digits))
            throw new DomainException("CPF com dígito verificador inválido.");

        return new Cpf(digits);
    }

    public string Formatado() =>
        $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";

    private static bool ValidarDigitos(string digits)
    {
        var nums = digits.Select(c => c - '0').ToArray();

        var sum1 = 0;
        for (var i = 0; i < 9; i++) sum1 += nums[i] * (10 - i);
        var d1 = sum1 * 10 % 11 % 10;
        if (d1 != nums[9]) return false;

        var sum2 = 0;
        for (var i = 0; i < 10; i++) sum2 += nums[i] * (11 - i);
        var d2 = sum2 * 10 % 11 % 10;
        return d2 == nums[10];
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Numero;
    }

    public override string ToString() => Formatado();

    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigitos();
}
