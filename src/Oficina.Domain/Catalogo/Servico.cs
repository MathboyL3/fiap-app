using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Catalogo;

public sealed class Servico : Entity
{
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public Money ValorBase { get; private set; }
    public TimeSpan TempoEstimado { get; private set; }
    public bool Ativo { get; private set; }

    private Servico() { Nome = Descricao = null!; ValorBase = null!; }

    public static Servico Cadastrar(string nome, string descricao, Money valorBase, TimeSpan tempoEstimado)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do serviço é obrigatório.");
        if (tempoEstimado <= TimeSpan.Zero)
            throw new DomainException("Tempo estimado deve ser positivo.");
        return new Servico
        {
            Nome = nome.Trim(),
            Descricao = (descricao ?? "").Trim(),
            ValorBase = valorBase,
            TempoEstimado = tempoEstimado,
            Ativo = true
        };
    }

    public void Atualizar(string? nome, string? descricao, Money? valorBase, TimeSpan? tempoEstimado)
    {
        if (!string.IsNullOrWhiteSpace(nome)) Nome = nome.Trim();
        if (descricao is not null) Descricao = descricao.Trim();
        if (valorBase is not null) ValorBase = valorBase;
        if (tempoEstimado.HasValue)
        {
            if (tempoEstimado.Value <= TimeSpan.Zero)
                throw new DomainException("Tempo estimado deve ser positivo.");
            TempoEstimado = tempoEstimado.Value;
        }
    }

    public void Inativar() => Ativo = false;
    public void Reativar() => Ativo = true;
}
