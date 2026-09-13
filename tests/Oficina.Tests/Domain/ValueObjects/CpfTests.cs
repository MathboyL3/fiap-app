using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("11144477735")]
    [InlineData("111.444.777-35")]
    [InlineData(" 111 444 777 35 ")]
    public void Create_ComCpfValido_DeveAceitarComOuSemFormatacao(string entrada)
    {
        var cpf = Cpf.Create(entrada);
        cpf.Numero.Should().Be("11144477735");
        cpf.Formatado().Should().Be("111.444.777-35");
    }

    [Fact]
    public void Create_ComStringVazia_DeveLancarDomainException()
    {
        Action act = () => Cpf.Create("");
        act.Should().Throw<DomainException>().WithMessage("*vazio*");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void Create_ComQuantidadeErradaDeDigitos_DeveLancar(string entrada)
    {
        Action act = () => Cpf.Create(entrada);
        act.Should().Throw<DomainException>().WithMessage("*11 dígitos*");
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    public void Create_ComTodosDigitosIguais_DeveLancar(string entrada)
    {
        Action act = () => Cpf.Create(entrada);
        act.Should().Throw<DomainException>().WithMessage("*iguais*");
    }

    [Fact]
    public void Create_ComDigitoVerificadorInvalido_DeveLancar()
    {
        Action act = () => Cpf.Create("11144477734");
        act.Should().Throw<DomainException>().WithMessage("*verificador*");
    }

    [Fact]
    public void Igualdade_DoisCpfsIguais_DevemSerEquivalentes()
    {
        var a = Cpf.Create("11144477735");
        var b = Cpf.Create("111.444.777-35");
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
