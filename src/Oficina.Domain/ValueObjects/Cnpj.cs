using System.Text.RegularExpressions;
using Oficina.Domain.Common;

namespace Oficina.Domain.ValueObjects;

public sealed partial class Cnpj : ValueObject
{
    public string Numero { get; }

    private Cnpj(string numero) => Numero = numero;

    public static Cnpj Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("CNPJ não pode ser vazio.");

        var digits = NaoDigitos().Replace(input, "");

        if (digits.Length != 14)
            throw new DomainException("CNPJ deve conter 14 dígitos.");

        if (digits.Distinct().Count() == 1)
            throw new DomainException("CNPJ inválido (todos os dígitos iguais).");

        if (!ValidarDigitos(digits))
            throw new DomainException("CNPJ com dígito verificador inválido.");

        return new Cnpj(digits);
    }

    public string Formatado() =>
        $"{Numero[..2]}.{Numero[2..5]}.{Numero[5..8]}/{Numero[8..12]}-{Numero[12..]}";

    private static bool ValidarDigitos(string digits)
    {
        var nums = digits.Select(c => c - '0').ToArray();
        int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum1 = 0;
        for (var i = 0; i < 12; i++) sum1 += nums[i] * mult1[i];
        var rem1 = sum1 % 11;
        var d1 = rem1 < 2 ? 0 : 11 - rem1;
        if (d1 != nums[12]) return false;

        var sum2 = 0;
        for (var i = 0; i < 13; i++) sum2 += nums[i] * mult2[i];
        var rem2 = sum2 % 11;
        var d2 = rem2 < 2 ? 0 : 11 - rem2;
        return d2 == nums[13];
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Numero;
    }

    public override string ToString() => Formatado();

    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigitos();
}
