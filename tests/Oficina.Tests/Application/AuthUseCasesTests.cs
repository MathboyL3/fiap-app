using FluentAssertions;
using Moq;
using Oficina.Application.Auth;
using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Auth;
using Oficina.Domain.Common;
using Oficina.Domain.Identidade;

namespace Oficina.Tests.Application;

public class AuthUseCasesTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private AuthUseCases UC() => new(_repo.Object, _hasher.Object, _jwt.Object, _uow.Object);

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        var u = Usuario.Criar("X", "x@y.com", "hash", Role.Atendente);
        _repo.Setup(r => r.ObterPorEmailAsync("x@y.com", default)).ReturnsAsync(u);
        _hasher.Setup(h => h.Verify("12345678", "hash")).Returns(true);
        _jwt.Setup(j => j.Generate(u.Id, u.Email, "Atendente")).Returns(("token", 3600));

        var resp = await UC().LoginAsync(new LoginRequest("x@y.com", "12345678"));

        resp.AccessToken.Should().Be("token");
        resp.TokenType.Should().Be("Bearer");
        resp.ExpiresInSeconds.Should().Be(3600);
    }

    [Fact]
    public async Task Login_UsuarioInexistente_DeveLancar()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((Usuario?)null);
        Func<Task> act = () => UC().LoginAsync(new LoginRequest("x@y.com", "12345678"));
        await act.Should().ThrowAsync<AppException>().WithMessage("*inválidas*");
    }

    [Fact]
    public async Task Login_SenhaErrada_DeveLancar()
    {
        var u = Usuario.Criar("X", "x@y.com", "hash", Role.Atendente);
        _repo.Setup(r => r.ObterPorEmailAsync("x@y.com", default)).ReturnsAsync(u);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        Func<Task> act = () => UC().LoginAsync(new LoginRequest("x@y.com", "wrong123"));
        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Criar_ComEmailExistente_DeveLancarConflict()
    {
        var existente = Usuario.Criar("X", "x@y.com", "hash", Role.Atendente);
        _repo.Setup(r => r.ObterPorEmailAsync("x@y.com", default)).ReturnsAsync(existente);

        Func<Task> act = () => UC().CriarAsync(new CriarUsuarioRequest("Y", "x@y.com", "12345678", "Gerente"));
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Criar_ComRoleInvalida_DeveLancar()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((Usuario?)null);
        Func<Task> act = () => UC().CriarAsync(new CriarUsuarioRequest("X", "x@y.com", "12345678", "Hacker"));
        await act.Should().ThrowAsync<AppException>().WithMessage("*Role inválida*");
    }

    [Fact]
    public async Task Criar_ComSenhaCurta_DeveLancar()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((Usuario?)null);
        Func<Task> act = () => UC().CriarAsync(new CriarUsuarioRequest("X", "x@y.com", "1234", "Gerente"));
        await act.Should().ThrowAsync<AppException>().WithMessage("*8 caracteres*");
    }
}
