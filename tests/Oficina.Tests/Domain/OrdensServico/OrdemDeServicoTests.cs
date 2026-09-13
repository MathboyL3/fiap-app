using FluentAssertions;
using Oficina.Domain.Common;
using Oficina.Domain.OrdensServico;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Domain.OrdensServico;

public class OrdemDeServicoTests
{
    private readonly Guid _clienteId = Guid.NewGuid();
    private readonly Guid _veiculoId = Guid.NewGuid();

    private OrdemDeServico NovaOS() => OrdemDeServico.Abrir(_clienteId, _veiculoId);

    [Fact]
    public void Abrir_DeveIniciarComStatusRecebida()
    {
        var os = NovaOS();
        os.Status.Should().Be(StatusOS.Recebida);
        os.ValorTotal.Amount.Should().Be(0);
        os.Historico.Should().HaveCount(1);
    }

    [Fact]
    public void Abrir_SemCliente_DeveLancar()
    {
        Action act = () => OrdemDeServico.Abrir(Guid.Empty, _veiculoId);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IncluirServico_DeveAumentarValorTotal()
    {
        var os = NovaOS();
        os.IncluirServico(Guid.NewGuid(), "Troca de óleo", Money.Brl(100), 1);
        os.IncluirServico(Guid.NewGuid(), "Alinhamento", Money.Brl(80), 2);
        os.ValorTotal.Amount.Should().Be(260);
    }

    [Fact]
    public void IncluirPeca_DeveAumentarValorTotal()
    {
        var os = NovaOS();
        os.IncluirServico(Guid.NewGuid(), "Diagnóstico", Money.Brl(50), 1);
        os.IncluirPeca(Guid.NewGuid(), "Filtro de óleo", Money.Brl(30), 2);
        os.ValorTotal.Amount.Should().Be(110);
    }

    [Fact]
    public void RemoverServico_DeveRecalcularTotal()
    {
        var os = NovaOS();
        os.IncluirServico(Guid.NewGuid(), "S1", Money.Brl(100), 1);
        var item = os.Servicos.First();
        os.RemoverServico(item.Id);
        os.Servicos.Should().BeEmpty();
        os.ValorTotal.Amount.Should().Be(0);
    }

    [Fact]
    public void RemoverServicoInexistente_DeveLancar()
    {
        var os = NovaOS();
        Action act = () => os.RemoverServico(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*não encontrado*");
    }

    [Fact]
    public void IniciarDiagnostico_DePassoInicial_DeveTransicionarParaEmDiagnostico()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("Avaliando freios");
        os.Status.Should().Be(StatusOS.EmDiagnostico);
        os.ObservacoesDiagnostico.Should().Be("Avaliando freios");
    }

    [Fact]
    public void EnviarParaAprovacao_SemItens_DeveLancar()
    {
        var os = NovaOS();
        os.IniciarDiagnostico();
        Action act = () => os.EnviarParaAprovacao();
        act.Should().Throw<DomainException>().WithMessage("*sem itens*");
    }

    [Fact]
    public void FluxoCompleto_DeveTransicionarTodosOsStatusEmOrdem()
    {
        var os = NovaOS();
        os.IniciarDiagnostico("ok");
        os.IncluirServico(Guid.NewGuid(), "Troca de óleo", Money.Brl(100), 1);
        os.EnviarParaAprovacao();
        os.Status.Should().Be(StatusOS.AguardandoAprovacao);
        os.AprovarOrcamento();
        os.Status.Should().Be(StatusOS.EmExecucao);
        os.IniciadaExecucaoEm.Should().NotBeNull();
        os.Finalizar();
        os.Status.Should().Be(StatusOS.Finalizada);
        os.FinalizadaEm.Should().NotBeNull();
        os.TempoExecucao.Should().NotBeNull();
        os.Entregar();
        os.Status.Should().Be(StatusOS.Entregue);
        os.EntregueEm.Should().NotBeNull();
    }

    [Fact]
    public void TransicaoForaDeOrdem_DeveLancar()
    {
        var os = NovaOS();
        Action act = () => os.AprovarOrcamento();
        act.Should().Throw<DomainException>().WithMessage("*Transição inválida*");
    }

    [Fact]
    public void IncluirServico_AposEnviarParaAprovacao_DeveLancar()
    {
        var os = NovaOS();
        os.IniciarDiagnostico();
        os.IncluirServico(Guid.NewGuid(), "S1", Money.Brl(50), 1);
        os.EnviarParaAprovacao();
        Action act = () => os.IncluirServico(Guid.NewGuid(), "S2", Money.Brl(30), 1);
        act.Should().Throw<DomainException>().WithMessage("*AguardandoAprovacao*");
    }

    [Fact]
    public void RejeitarOrcamento_DeveTransicionarParaCancelada()
    {
        var os = NovaOS();
        os.IniciarDiagnostico();
        os.IncluirServico(Guid.NewGuid(), "S1", Money.Brl(50), 1);
        os.EnviarParaAprovacao();
        os.RejeitarOrcamento("Cliente não autorizou.");
        os.Status.Should().Be(StatusOS.Cancelada);
    }

    [Fact]
    public void Cancelar_OSEntregue_DeveLancar()
    {
        var os = NovaOS();
        os.IniciarDiagnostico();
        os.IncluirServico(Guid.NewGuid(), "S1", Money.Brl(50), 1);
        os.EnviarParaAprovacao();
        os.AprovarOrcamento();
        os.Finalizar();
        os.Entregar();
        Action act = () => os.Cancelar("tarde");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancelar_AntesDaEntrega_DevePermitir()
    {
        var os = NovaOS();
        os.IniciarDiagnostico();
        os.Cancelar("desistência do cliente");
        os.Status.Should().Be(StatusOS.Cancelada);
        os.Historico.Last().Para.Should().Be(StatusOS.Cancelada);
    }
}
