using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.Estoque;

namespace Oficina.Tests.Domain.Estoque;

public class EstoquePecaTests
{
    private readonly Guid _pecaId = Guid.NewGuid();

    [Fact]
    public void Criar_ComQuantidadeInicialPositiva_DeveRegistrarMovimentacaoEntrada()
    {
        var e = EstoquePeca.Criar(_pecaId, quantidadeInicial: 10, limiteMinimo: 2);
        e.QuantidadeDisponivel.Should().Be(10);
        e.QuantidadeReservada.Should().Be(0);
        e.LimiteMinimo.Should().Be(2);
        e.Movimentacoes.Should().HaveCount(1);
        e.Movimentacoes.First().Tipo.Should().Be(TipoMovimentacao.Entrada);
    }

    [Fact]
    public void Reservar_ComEstoqueSuficiente_DeveDecrementarDisponivelEAumentarReservado()
    {
        var e = EstoquePeca.Criar(_pecaId, 10);
        e.Reservar(3, "OS:abc");
        e.QuantidadeDisponivel.Should().Be(7);
        e.QuantidadeReservada.Should().Be(3);
        e.Total.Should().Be(10);
    }

    [Fact]
    public void Reservar_ComEstoqueInsuficiente_DeveLancar()
    {
        var e = EstoquePeca.Criar(_pecaId, 2);
        Action act = () => e.Reservar(5, "OS:x");
        act.Should().Throw<DomainException>().WithMessage("*Estoque insuficiente*");
    }

    [Fact]
    public void EstornarReserva_DeveDevolverParaDisponivel()
    {
        var e = EstoquePeca.Criar(_pecaId, 10);
        e.Reservar(5, "OS:x");
        e.EstornarReserva(2, "OS:x");
        e.QuantidadeDisponivel.Should().Be(7);
        e.QuantidadeReservada.Should().Be(3);
    }

    [Fact]
    public void EstornarReserva_AlemDoReservado_DeveLancar()
    {
        var e = EstoquePeca.Criar(_pecaId, 10);
        e.Reservar(2, "OS:x");
        Action act = () => e.EstornarReserva(5, "OS:x");
        act.Should().Throw<DomainException>().WithMessage("*reserva suficiente*");
    }

    [Fact]
    public void ConsumirReservado_DeveRemoverDoReservadoSemDevolverAoDisponivel()
    {
        var e = EstoquePeca.Criar(_pecaId, 10);
        e.Reservar(5, "OS:x");
        e.ConsumirReservado(3, "OS:x");
        e.QuantidadeDisponivel.Should().Be(5);
        e.QuantidadeReservada.Should().Be(2);
        e.Total.Should().Be(7);
    }

    [Fact]
    public void Reabastecer_DeveAdicionarAoDisponivel()
    {
        var e = EstoquePeca.Criar(_pecaId, 5);
        e.Reabastecer(10, "compra-001");
        e.QuantidadeDisponivel.Should().Be(15);
        e.Movimentacoes.Should().HaveCount(2);
    }

    [Fact]
    public void EstoqueBaixo_QuandoTotalAbaixoDoLimite_DeveSerTrue()
    {
        var e = EstoquePeca.Criar(_pecaId, 3, limiteMinimo: 5);
        e.EstoqueBaixo.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reservar_ComQuantidadeNaoPositiva_DeveLancar(int qtd)
    {
        var e = EstoquePeca.Criar(_pecaId, 10);
        Action act = () => e.Reservar(qtd, "x");
        act.Should().Throw<DomainException>();
    }
}
