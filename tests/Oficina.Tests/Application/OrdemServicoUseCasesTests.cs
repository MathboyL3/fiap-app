using FluentAssertions;
using Moq;
using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Notificacoes;
using Oficina.Application.UseCases.OrdensServico;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.Estoque;
using Oficina.Domain.OrdensServico;
using Oficina.Domain.ValueObjects;
using Oficina.Domain.Veiculos;

namespace Oficina.Tests.Application;

public class OrdemServicoUseCasesTests
{
    private readonly Mock<IOrdemDeServicoRepository> _osRepo = new();
    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Mock<IVeiculoRepository> _veiculos = new();
    private readonly Mock<IServicoRepository> _servicos = new();
    private readonly Mock<IPecaRepository> _pecas = new();
    private readonly Mock<IEstoquePecaRepository> _estoque = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<INotificador> _notificador = new();

    private OrdemServicoUseCases UC() => new(
        _osRepo.Object, _clientes.Object, _veiculos.Object,
        _servicos.Object, _pecas.Object, _estoque.Object, _uow.Object, _notificador.Object);

    private static Cliente NovoCliente() =>
        Cliente.Cadastrar("Joao", Documento.Parse("11144477735"), "joao@x.com", "1234");

    private static Veiculo NovoVeiculo(Guid clienteId) =>
        Veiculo.Cadastrar(clienteId, Placa.Create("ABC1D23"), "Fiat", "Mobi", 2022);

    [Fact]
    public async Task Abrir_ClienteInexistente_DeveLancarNotFound()
    {
        _clientes.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Cliente?)null);

        Func<Task> act = () => UC().AbrirAsync(new AbrirOSRequest(Guid.NewGuid(), Guid.NewGuid()));
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Abrir_VeiculoDeOutroCliente_DeveLancarConflict()
    {
        var c = NovoCliente();
        var v = NovoVeiculo(Guid.NewGuid()); // veículo de outro cliente
        _clientes.Setup(r => r.ObterPorIdAsync(c.Id, default)).ReturnsAsync(c);
        _veiculos.Setup(r => r.ObterPorIdAsync(v.Id, default)).ReturnsAsync(v);

        Func<Task> act = () => UC().AbrirAsync(new AbrirOSRequest(c.Id, v.Id));
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Abrir_DadosValidos_DevePersistirECommitar()
    {
        var c = NovoCliente();
        var v = NovoVeiculo(c.Id);
        _clientes.Setup(r => r.ObterPorIdAsync(c.Id, default)).ReturnsAsync(c);
        _veiculos.Setup(r => r.ObterPorIdAsync(v.Id, default)).ReturnsAsync(v);

        var resp = await UC().AbrirAsync(new AbrirOSRequest(c.Id, v.Id));

        resp.Status.Should().Be(StatusOS.Recebida.ToString());
        resp.ClienteId.Should().Be(c.Id);
        _osRepo.Verify(r => r.AdicionarAsync(It.IsAny<OrdemDeServico>(), default), Times.Once);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task IncluirPeca_SemEstoque_DeveLancarNotFound()
    {
        var os = OrdemDeServico.Abrir(Guid.NewGuid(), Guid.NewGuid());
        var peca = Peca.Cadastrar("Filtro", "F1", "un", Money.Brl(20));
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);
        _pecas.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);
        _estoque.Setup(r => r.ObterPorPecaIdAsync(peca.Id, default)).ReturnsAsync((EstoquePeca?)null);

        Func<Task> act = () => UC().IncluirPecaAsync(os.Id, new IncluirItemPecaRequest(peca.Id, 1));
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task IncluirPeca_ComEstoqueSuficiente_DeveReservarEAtualizarOS()
    {
        var os = OrdemDeServico.Abrir(Guid.NewGuid(), Guid.NewGuid());
        var peca = Peca.Cadastrar("Filtro", "F1", "un", Money.Brl(20));
        var estoque = EstoquePeca.Criar(peca.Id, 10);
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);
        _pecas.Setup(r => r.ObterPorIdAsync(peca.Id, default)).ReturnsAsync(peca);
        _estoque.Setup(r => r.ObterPorPecaIdAsync(peca.Id, default)).ReturnsAsync(estoque);

        var resp = await UC().IncluirPecaAsync(os.Id, new IncluirItemPecaRequest(peca.Id, 3));

        resp.Pecas.Should().HaveCount(1);
        estoque.QuantidadeReservada.Should().Be(3);
        estoque.QuantidadeDisponivel.Should().Be(7);
    }

    [Fact]
    public async Task FinalizarOS_DeveConsumirReservasDoEstoque()
    {
        var os = OrdemDeServico.Abrir(Guid.NewGuid(), Guid.NewGuid());
        var peca = Peca.Cadastrar("Filtro", "F1", "un", Money.Brl(20));
        var estoque = EstoquePeca.Criar(peca.Id, 10);

        // simular fluxo: incluir peça (que reserva no estoque) e seguir até execução
        os.IncluirPeca(peca.Id, peca.Nome, peca.Valor, 2);
        estoque.Reservar(2, "OS:" + os.Id);
        os.IniciarDiagnostico();
        os.EnviarParaAprovacao();
        os.AprovarOrcamento();

        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);
        _estoque.Setup(r => r.ObterPorPecaIdAsync(peca.Id, default)).ReturnsAsync(estoque);

        var resp = await UC().FinalizarAsync(os.Id);

        resp.Status.Should().Be(StatusOS.Finalizada.ToString());
        estoque.QuantidadeReservada.Should().Be(0);
        estoque.QuantidadeDisponivel.Should().Be(8);
    }

    [Fact]
    public async Task CalcularTempoMedio_SemOSFinalizadas_DeveRetornarZero()
    {
        _osRepo.Setup(r => r.ListarFinalizadasNoIntervaloAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), default))
            .ReturnsAsync(Array.Empty<OrdemDeServico>());

        var resp = await UC().CalcularTempoMedioAsync(null, null);
        resp.Quantidade.Should().Be(0);
        resp.TempoMedioMinutos.Should().Be(0);
    }

    [Fact]
    public async Task ListarPainel_DeveDelegarAoRepositorioEMapear()
    {
        var os = OrdemDeServico.Abrir(Guid.NewGuid(), Guid.NewGuid());
        _osRepo.Setup(r => r.ListarPainelAsync(0, 20, default))
            .ReturnsAsync(new[] { os });

        var resp = await UC().ListarPainelAsync(0, 20);

        resp.Should().HaveCount(1);
        resp[0].Id.Should().Be(os.Id);
        _osRepo.Verify(r => r.ListarPainelAsync(0, 20, default), Times.Once);
    }

    [Fact]
    public async Task Webhook_Aprovado_DeveAprovarOrcamentoENotificar()
    {
        var os = OrdemDeServico.Abrir(Guid.NewGuid(), Guid.NewGuid());
        os.IncluirServico(Guid.NewGuid(), "Troca de óleo", Money.Brl(100), 1);
        os.IniciarDiagnostico();
        os.EnviarParaAprovacao();
        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);

        var resp = await UC().ProcessarWebhookAprovacaoAsync(new WebhookAprovacaoRequest(os.Id, true, null));

        resp.Status.Should().Be(StatusOS.EmExecucao.ToString());
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
        _notificador.Verify(n => n.NotificarMudancaStatusAsync(
            os.Id, StatusOS.EmExecucao.ToString(), It.IsAny<string>(), default), Times.Once);
    }

    [Fact]
    public async Task Webhook_Rejeitado_DeveRejeitarEstornarReservasENotificar()
    {
        var os = OrdemDeServico.Abrir(Guid.NewGuid(), Guid.NewGuid());
        var peca = Peca.Cadastrar("Filtro", "F1", "un", Money.Brl(20));
        var estoque = EstoquePeca.Criar(peca.Id, 10);
        os.IncluirPeca(peca.Id, peca.Nome, peca.Valor, 2);
        estoque.Reservar(2, "OS:" + os.Id);
        os.IniciarDiagnostico();
        os.EnviarParaAprovacao();

        _osRepo.Setup(r => r.ObterPorIdAsync(os.Id, default)).ReturnsAsync(os);
        _estoque.Setup(r => r.ObterPorPecaIdAsync(peca.Id, default)).ReturnsAsync(estoque);

        var resp = await UC().ProcessarWebhookAprovacaoAsync(
            new WebhookAprovacaoRequest(os.Id, false, "Cliente não aprovou"));

        resp.Status.Should().Be(StatusOS.Cancelada.ToString());
        estoque.QuantidadeReservada.Should().Be(0);
        _notificador.Verify(n => n.NotificarMudancaStatusAsync(
            os.Id, StatusOS.Cancelada.ToString(), It.IsAny<string>(), default), Times.Once);
    }
}
