using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Oficina.Application.DTOs;

namespace Oficina.Tests.Integration;

public sealed class AuthIntegrationTests : IClassFixture<OficinaApiFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(OficinaApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Bootstrap_ComDadosValidos_DeveCriarUsuario()
    {
        var request = new CriarUsuarioRequest(
            "Gerente Integracao",
            $"gerente-{Guid.NewGuid():N}@oficina.test",
            "Senha123!",
            "Gerente");

        var response = await _client.PostAsJsonAsync("/api/auth/bootstrap", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario.Should().NotBeNull();
        usuario!.Email.Should().Be(request.Email.ToLowerInvariant());
        usuario.Role.Should().Be("Gerente");
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarTokenBearer()
    {
        var email = $"login-{Guid.NewGuid():N}@oficina.test";
        var senha = "Senha123!";

        await _client.PostAsJsonAsync("/api/auth/bootstrap",
            new CriarUsuarioRequest("Gerente Login", email, senha, "Gerente"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, senha));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        token.Should().NotBeNull();
        token!.TokenType.Should().Be("Bearer");
        token.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.ExpiresInSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"inexistente-{Guid.NewGuid():N}@oficina.test", "Senha123!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
