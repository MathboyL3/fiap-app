using Oficina.Domain.Common;

namespace Oficina.Domain.Identidade;

public sealed class Usuario : Entity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public Role Role { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Usuario() { Nome = Email = SenhaHash = null!; }

    public static Usuario Criar(string nome, string email, string senhaHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("E-mail inválido.");
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("Hash da senha é obrigatório.");
        return new Usuario
        {
            Nome = nome.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            SenhaHash = senhaHash,
            Role = role,
            CriadoEm = DateTime.UtcNow
        };
    }
}
