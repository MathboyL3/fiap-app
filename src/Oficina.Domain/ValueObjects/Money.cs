using Oficina.Domain.Common;

namespace Oficina.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Brl(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Valor monetário não pode ser negativo.");
        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), "BRL");
    }

    public static Money Zero => Brl(0);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Moedas diferentes não podem ser somadas.");
        return Brl(Amount + other.Amount);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("Quantidade não pode ser negativa.");
        return Brl(Amount * quantity);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:0.00}";
}
