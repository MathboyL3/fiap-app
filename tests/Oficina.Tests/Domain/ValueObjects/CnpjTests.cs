using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.ValueObjects;

public class CnpjTests
{
    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    public void Create_ComCnpjValido_DeveAceitar(string entrada)
    {
        var cnpj = Cnpj.Create(entrada);
        cnpj.Numero.Should().Be("11222333000181");
        cnpj.Formatado().Should().Be("11.222.333/0001-81");
    }

    [Fact]
    public void Create_ComCnpjInvalido_DeveLancar()
    {
        Action act = () => Cnpj.Create("11222333000180");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012345")]
    public void Create_ComQuantidadeErrada_DeveLancar(string entrada)
    {
        Action act = () => Cnpj.Create(entrada);
        act.Should().Throw<DomainException>().WithMessage("*14 dígitos*");
    }

    [Fact]
    public void Create_ComTodosDigitosIguais_DeveLancar()
    {
        Action act = () => Cnpj.Create("11111111111111");
        act.Should().Throw<DomainException>().WithMessage("*iguais*");
    }
}
