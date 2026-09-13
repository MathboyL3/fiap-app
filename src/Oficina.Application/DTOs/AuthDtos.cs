namespace Oficina.Application.DTOs;

public record LoginRequest(string Email, string Senha);

public record CriarUsuarioRequest(string Nome, string Email, string Senha, string Role);

public record TokenResponse(string AccessToken, string TokenType, int ExpiresInSeconds);

public record UsuarioResponse(Guid Id, string Nome, string Email, string Role);
