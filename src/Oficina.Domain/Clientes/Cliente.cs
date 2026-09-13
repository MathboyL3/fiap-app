using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.Clientes;

public sealed class Cliente : Entity
{
    public string Nome { get; private set; }
    public Documento Documento { get; private set; }
    public string Email { get; private set; }
    public string Telefone { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private Cliente() { Nome = Email = Telefone = null!; Documento = null!; }

    public static Cliente Cadastrar(string nome, Documento documento, string email, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("E-mail do cliente é inválido.");
        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainException("Telefone do cliente é obrigatório.");

        return new Cliente
        {
            Nome = nome.Trim(),
            Documento = documento,
            Email = email.Trim().ToLowerInvariant(),
            Telefone = telefone.Trim(),
            CriadoEm = DateTime.UtcNow
        };
    }

    public void Atualizar(string nome, string email, string telefone)
    {
        if (!string.IsNullOrWhiteSpace(nome)) Nome = nome.Trim();
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!email.Contains('@')) throw new DomainException("E-mail inválido.");
            Email = email.Trim().ToLowerInvariant();
        }
        if (!string.IsNullOrWhiteSpace(telefone)) Telefone = telefone.Trim();
    }
}
