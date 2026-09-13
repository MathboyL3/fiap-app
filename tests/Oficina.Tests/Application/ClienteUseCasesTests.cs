using FluentAssertions;
using Moq;
using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Clientes;
using Oficina.Domain.Clientes;
using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Tests.Application;

public class ClienteUseCasesTests
{
    private readonly Mock<IClienteRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private ClienteUseCases UC() => new(_repo.Object, _uow.Object);

    [Fact]
    public async Task Criar_ComDocumentoJaExistente_DeveLancarConflict()
    {
        var existente = Cliente.Cadastrar("X", Documento.Parse("11144477735"), "x@y.com", "1");
        _repo.Setup(r => r.ObterPorDocumentoAsync(It.IsAny<Documento>(), default)).ReturnsAsync(existente);

        Func<Task> act = () => UC().CriarAsync(new CriarClienteRequest("Y", "11144477735", "y@z.com", "2"));
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Criar_DadosValidos_DevePersistirECommitar()
    {
        _repo.Setup(r => r.ObterPorDocumentoAsync(It.IsAny<Documento>(), default)).ReturnsAsync((Cliente?)null);

        var resp = await UC().CriarAsync(new CriarClienteRequest("Joao", "11144477735", "joao@x.com", "1"));

        resp.Nome.Should().Be("Joao");
        _repo.Verify(r => r.AdicionarAsync(It.IsAny<Cliente>(), default), Times.Once);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task Obter_NaoExistente_DeveLancarNotFound()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Cliente?)null);
        Func<Task> act = () => UC().ObterAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
