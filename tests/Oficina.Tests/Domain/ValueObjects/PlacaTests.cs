using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.ValueObjects;

public class PlacaTests
{
    [Theory]
    [InlineData("ABC1234")]
    [InlineData("ABC-1234")]
    [InlineData("abc1234")]
    public void Create_ComFormatoAntigo_DeveAceitar(string entrada)
    {
        var p = Placa.Create(entrada);
        p.Valor.Should().Be("ABC1234");
        p.Formatada().Should().Be("ABC-1234");
    }

    [Theory]
    [InlineData("ABC1D23")]
    [InlineData("abc1d23")]
    public void Create_ComFormatoMercosul_DeveAceitar(string entrada)
    {
        var p = Placa.Create(entrada);
        p.Valor.Should().Be("ABC1D23");
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC12")]
    [InlineData("12345AB")]
    public void Create_ComFormatoInvalido_DeveLancar(string entrada)
    {
        Action act = () => Placa.Create(entrada);
        act.Should().Throw<DomainException>();
    }
}
