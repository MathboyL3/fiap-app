namespace Oficina.Application.Auth;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verify(string senha, string hash);
}

public interface IJwtTokenService
{
    (string token, int expiresInSeconds) Generate(Guid usuarioId, string email, string role);
}
