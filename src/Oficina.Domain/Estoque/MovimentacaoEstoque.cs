using Oficina.Domain.Common;

namespace Oficina.Domain.Estoque;

public sealed class MovimentacaoEstoque : Entity
{
    public Guid EstoquePecaId { get; private set; }
    public TipoMovimentacao Tipo { get; private set; }
    public int Quantidade { get; private set; }
    public DateTime OcorreuEm { get; private set; }
    public string? Referencia { get; private set; }

    private MovimentacaoEstoque() { }

    internal static MovimentacaoEstoque Criar(Guid estoquePecaId, TipoMovimentacao tipo, int quantidade, string? referencia)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade da movimentação deve ser positiva.");

        return new MovimentacaoEstoque
        {
            EstoquePecaId = estoquePecaId,
            Tipo = tipo,
            Quantidade = quantidade,
            OcorreuEm = DateTime.UtcNow,
            Referencia = referencia
        };
    }
}
