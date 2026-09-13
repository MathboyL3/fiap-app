using FluentAssertions;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.Clientes;

public class ClienteTests
{
    private static Documento DocCpf() => Documento.Parse("11144477735");

    [Fact]
    public void Cadastrar_ComDadosValidos_DeveCriar()
    {
        var c = Cliente.Cadastrar("João", DocCpf(), "joao@example.com", "11999990000");
        c.Nome.Should().Be("João");
        c.Email.Should().Be("joao@example.com");
        c.Telefone.Should().Be("11999990000");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cadastrar_ComNomeVazio_DeveLancar(string nome)
    {
        Action act = () => Cliente.Cadastrar(nome, DocCpf(), "x@y.com", "1234");
        act.Should().Throw<DomainException>().WithMessage("*Nome*");
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("")]
    public void Cadastrar_ComEmailInvalido_DeveLancar(string email)
    {
        Action act = () => Cliente.Cadastrar("Joao", DocCpf(), email, "1234");
        act.Should().Throw<DomainException>().WithMessage("*mail*");
    }

    [Fact]
    public void Atualizar_ComCamposParciais_DeveAplicarSomenteOsInformados()
    {
        var c = Cliente.Cadastrar("Joao", DocCpf(), "j@a.com", "111");
        c.Atualizar("Joao Silva", "", "");
        c.Nome.Should().Be("Joao Silva");
        c.Email.Should().Be("j@a.com");
    }
}
