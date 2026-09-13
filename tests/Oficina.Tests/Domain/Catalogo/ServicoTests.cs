using FluentAssertions;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.Catalogo;

public class ServicoTests
{
    [Fact]
    public void Cadastrar_ComDadosValidos_DeveCriar()
    {
        var s = Servico.Cadastrar("Troca de óleo", "Síntético 5W30", Money.Brl(100), TimeSpan.FromMinutes(45));
        s.Nome.Should().Be("Troca de óleo");
        s.ValorBase.Amount.Should().Be(100);
        s.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Cadastrar_ComTempoZero_DeveLancar()
    {
        Action act = () => Servico.Cadastrar("X", "", Money.Brl(10), TimeSpan.Zero);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Inativar_DeveDesativar()
    {
        var s = Servico.Cadastrar("X", "", Money.Brl(10), TimeSpan.FromMinutes(10));
        s.Inativar();
        s.Ativo.Should().BeFalse();
        s.Reativar();
        s.Ativo.Should().BeTrue();
    }

    [Fact]
    public void Atualizar_DeveAplicarApenasOsCampoInformados()
    {
        var s = Servico.Cadastrar("X", "desc", Money.Brl(10), TimeSpan.FromMinutes(30));
        s.Atualizar("Y", null, null, null);
        s.Nome.Should().Be("Y");
        s.Descricao.Should().Be("desc");
        s.ValorBase.Amount.Should().Be(10);
    }
}

public class PecaTests
{
    [Fact]
    public void Cadastrar_DeveNormalizarCodigoEmMaiusculas()
    {
        var p = Peca.Cadastrar("Filtro", "fil-001", "un", Money.Brl(20));
        p.Codigo.Should().Be("FIL-001");
    }

    [Fact]
    public void Inativar_DeveDesativar()
    {
        var p = Peca.Cadastrar("Filtro", "F1", "un", Money.Brl(20));
        p.Inativar();
        p.Ativo.Should().BeFalse();
    }
}
