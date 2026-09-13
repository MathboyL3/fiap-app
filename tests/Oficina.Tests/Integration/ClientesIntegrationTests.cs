using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Oficina.Application.DTOs;

namespace Oficina.Tests.Integration;

public sealed class ClientesIntegrationTests : IClassFixture<OficinaApiFactory>
{
    private readonly HttpClient _client;

    public ClientesIntegrationTests(OficinaApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Criar_SemToken_DeveRetornarUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/clientes",
            new CriarClienteRequest("Cliente Sem Token", "52998224725", "sem-token@oficina.test", "11999999999"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Criar_ComTokenValido_DevePersistirCliente()
    {
        await AutenticarComoGerenteAsync();
        var request = new CriarClienteRequest(
            "Cliente Integracao",
            "52998224725",
            $"cliente-{Guid.NewGuid():N}@oficina.test",
            "11999999999");

        var createResponse = await _client.PostAsJsonAsync("/api/clientes", request);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await createResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        criado.Should().NotBeNull();
        criado!.Nome.Should().Be(request.Nome);
        criado.Documento.Should().Be("52998224725");

        var getResponse = await _client.GetAsync($"/api/clientes/{criado.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var obtido = await getResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        obtido.Should().BeEquivalentTo(criado, options => options.ComparingByMembers<ClienteResponse>());
    }

    [Fact]
    public async Task Criar_ComDocumentoDuplicado_DeveRetornarConflict()
    {
        await AutenticarComoGerenteAsync();
        var documento = "93541134780";
        var primeiro = new CriarClienteRequest(
            "Cliente Um",
            documento,
            $"cliente-um-{Guid.NewGuid():N}@oficina.test",
            "11999999999");
        var duplicado = new CriarClienteRequest(
            "Cliente Dois",
            documento,
            $"cliente-dois-{Guid.NewGuid():N}@oficina.test",
            "11888888888");

        await _client.PostAsJsonAsync("/api/clientes", primeiro);
        var response = await _client.PostAsJsonAsync("/api/clientes", duplicado);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task AutenticarComoGerenteAsync()
    {
        var email = $"gerente-clientes-{Guid.NewGuid():N}@oficina.test";
        var senha = "Senha123!";

        await _client.PostAsJsonAsync("/api/auth/bootstrap",
            new CriarUsuarioRequest("Gerente Clientes", email, senha, "Gerente"));

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, senha));
        loginResponse.EnsureSuccessStatusCode();

        var token = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }
}
