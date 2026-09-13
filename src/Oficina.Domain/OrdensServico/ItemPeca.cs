using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.OrdensServico;

public sealed class ItemPeca : Entity
{
    public Guid OrdemDeServicoId { get; private set; }
    public Guid PecaId { get; private set; }
    public string Descricao { get; private set; }
    public Money ValorUnitario { get; private set; }
    public int Quantidade { get; private set; }

    public Money Subtotal => ValorUnitario.Multiply(Quantidade);

    private ItemPeca() { Descricao = null!; ValorUnitario = null!; }

    internal static ItemPeca Criar(Guid osId, Guid pecaId, string descricao, Money valorUnitario, int quantidade)
    {
        if (pecaId == Guid.Empty)
            throw new DomainException("Item de peça requer uma peça.");
        if (quantidade <= 0)
            throw new DomainException("Quantidade de item de peça deve ser positiva.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição do item de peça é obrigatória.");
        return new ItemPeca
        {
            OrdemDeServicoId = osId,
            PecaId = pecaId,
            Descricao = descricao.Trim(),
            ValorUnitario = valorUnitario,
            Quantidade = quantidade
        };
    }
}
