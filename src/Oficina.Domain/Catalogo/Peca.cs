using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Catalogo;

public sealed class Peca : Entity
{
    public string Nome { get; private set; }
    public string Codigo { get; private set; }
    public string Unidade { get; private set; }
    public Money Valor { get; private set; }
    public bool Ativo { get; private set; }

    private Peca() { Nome = Codigo = Unidade = null!; Valor = null!; }

    public static Peca Cadastrar(string nome, string codigo, string unidade, Money valor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da peça é obrigatório.");
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("Código da peça é obrigatório.");
        if (string.IsNullOrWhiteSpace(unidade))
            throw new DomainException("Unidade da peça é obrigatória.");
        return new Peca
        {
            Nome = nome.Trim(),
            Codigo = codigo.Trim().ToUpperInvariant(),
            Unidade = unidade.Trim(),
            Valor = valor,
            Ativo = true
        };
    }

    public void AtualizarValor(Money novoValor) => Valor = novoValor;
    public void Inativar() => Ativo = false;
    public void Reativar() => Ativo = true;
}
