using Oficina.Domain.Common;

namespace Oficina.Domain.ValueObjects;

public enum TipoDocumento { Cpf, Cnpj }

public sealed class Documento : ValueObject
{
    public TipoDocumento Tipo { get; }
    public string Numero { get; }

    private Documento(TipoDocumento tipo, string numero)
    {
        Tipo = tipo;
        Numero = numero;
    }

    public static Documento DeCpf(Cpf cpf) => new(TipoDocumento.Cpf, cpf.Numero);
    public static Documento DeCnpj(Cnpj cnpj) => new(TipoDocumento.Cnpj, cnpj.Numero);

    public static Documento Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new DomainException("Documento não pode ser vazio.");
        var digits = new string(input.Where(char.IsDigit).ToArray());
        return digits.Length switch
        {
            11 => DeCpf(Cpf.Create(digits)),
            14 => DeCnpj(Cnpj.Create(digits)),
            _ => throw new DomainException("Documento deve ter 11 (CPF) ou 14 (CNPJ) dígitos.")
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Tipo;
        yield return Numero;
    }

    public override string ToString() =>
        Tipo == TipoDocumento.Cpf ? Cpf.Create(Numero).Formatado() : Cnpj.Create(Numero).Formatado();
}
