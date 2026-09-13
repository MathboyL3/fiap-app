using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Veiculos;

namespace Oficina.Tests.Domain.Veiculos;

public class VeiculoTests
{
    [Fact]
    public void Cadastrar_ComDadosValidos_DeveCriar()
    {
        var v = Veiculo.Cadastrar(Guid.NewGuid(), Placa.Create("ABC1D23"), "Fiat", "Mobi", 2022);
        v.Marca.Should().Be("Fiat");
        v.Modelo.Should().Be("Mobi");
        v.Ano.Should().Be(2022);
    }

    [Fact]
    public void Cadastrar_SemCliente_DeveLancar()
    {
        Action act = () => Veiculo.Cadastrar(Guid.Empty, Placa.Create("ABC1D23"), "Fiat", "Mobi", 2022);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(3000)]
    public void Cadastrar_ComAnoForaDoIntervalo_DeveLancar(int ano)
    {
        Action act = () => Veiculo.Cadastrar(Guid.NewGuid(), Placa.Create("ABC1D23"), "Fiat", "Mobi", ano);
        act.Should().Throw<DomainException>().WithMessage("*Ano*");
    }
}
