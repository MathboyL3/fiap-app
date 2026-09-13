using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.ValueObjects;

public class DocumentoTests
{
    [Fact]
    public void Parse_ComCpfValido_DeveDeterminarTipoCorretamente()
    {
        var d = Documento.Parse("111.444.777-35");
        d.Tipo.Should().Be(TipoDocumento.Cpf);
        d.Numero.Should().Be("11144477735");
    }

    [Fact]
    public void Parse_ComCnpjValido_DeveDeterminarTipoCorretamente()
    {
        var d = Documento.Parse("11.222.333/0001-81");
        d.Tipo.Should().Be(TipoDocumento.Cnpj);
        d.Numero.Should().Be("11222333000181");
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("1234567890123")]
    public void Parse_ComTamanhoInvalido_DeveLancar(string entrada)
    {
        Action act = () => Documento.Parse(entrada);
        act.Should().Throw<DomainException>();
    }
}
