using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.OrdensServico;

public sealed class ItemServico : Entity
{
    public Guid OrdemDeServicoId { get; private set; }
    public Guid ServicoId { get; private set; }
    public string Descricao { get; private set; }
    public Money ValorUnitario { get; private set; }
    public int Quantidade { get; private set; }

    public Money Subtotal => ValorUnitario.Multiply(Quantidade);

    private ItemServico() { Descricao = null!; ValorUnitario = null!; }

    internal static ItemServico Criar(Guid osId, Guid servicoId, string descricao, Money valorUnitario, int quantidade)
    {
        if (servicoId == Guid.Empty)
            throw new DomainException("Item de serviço requer um serviço.");
        if (quantidade <= 0)
            throw new DomainException("Quantidade de item de serviço deve ser positiva.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição do item de serviço é obrigatória.");
        return new ItemServico
        {
            OrdemDeServicoId = osId,
            ServicoId = servicoId,
            Descricao = descricao.Trim(),
            ValorUnitario = valorUnitario,
            Quantidade = quantidade
        };
    }
}
