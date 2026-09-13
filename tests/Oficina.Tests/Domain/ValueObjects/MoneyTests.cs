using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Brl_ComValorValido_DeveCriarComArredondamento()
    {
        var m = Money.Brl(10.555m);
        m.Amount.Should().Be(10.56m);
        m.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Brl_ComValorNegativo_DeveLancar()
    {
        Action act = () => Money.Brl(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Add_DoisValoresMesmaMoeda_DeveSomar()
    {
        var a = Money.Brl(10);
        var b = Money.Brl(2.5m);
        a.Add(b).Amount.Should().Be(12.5m);
    }

    [Fact]
    public void Multiply_PorQuantidadePositiva_DeveCalcular()
    {
        Money.Brl(15).Multiply(3).Amount.Should().Be(45m);
    }

    [Fact]
    public void Multiply_PorQuantidadeNegativa_DeveLancar()
    {
        Action act = () => Money.Brl(10).Multiply(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Zero_DeveSerZeroBrl()
    {
        Money.Zero.Amount.Should().Be(0m);
        Money.Zero.Currency.Should().Be("BRL");
    }
}
