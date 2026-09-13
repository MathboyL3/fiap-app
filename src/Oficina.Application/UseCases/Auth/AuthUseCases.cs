using Oficina.Application.Auth;
using Oficina.Application.Common;
using Oficina.Application.DTOs;
using Oficina.Application.Mapping;
using Oficina.Domain.Common;
using Oficina.Domain.Identidade;

namespace Oficina.Application.UseCases.Auth;

public sealed class AuthUseCases
{
    private readonly IUsuarioRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IUnitOfWork _uow;

    public AuthUseCases(IUsuarioRepository repo, IPasswordHasher hasher, IJwtTokenService jwt, IUnitOfWork uow)
    {
        _repo = repo;
        _hasher = hasher;
        _jwt = jwt;
        _uow = uow;
    }

    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest req, CancellationToken ct = default)
    {
        var existente = await _repo.ObterPorEmailAsync(req.Email.Trim().ToLowerInvariant(), ct);
        if (existente is not null)
            throw new ConflictException($"Já existe usuário com e-mail {req.Email}.");

        if (!Enum.TryParse<Role>(req.Role, true, out var role))
            throw new AppException($"Role inválida: {req.Role}. Valores válidos: Atendente, Mecanico, Gerente.");

        if (string.IsNullOrWhiteSpace(req.Senha) || req.Senha.Length < 8)
            throw new AppException("Senha deve ter ao menos 8 caracteres.");

        var hash = _hasher.Hash(req.Senha);
        var usuario = Usuario.Criar(req.Nome, req.Email, hash, role);
        await _repo.AdicionarAsync(usuario, ct);
        await _uow.CommitAsync(ct);
        return usuario.ToResponse();
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var usuario = await _repo.ObterPorEmailAsync(req.Email.Trim().ToLowerInvariant(), ct);
        if (usuario is null || !_hasher.Verify(req.Senha, usuario.SenhaHash))
            throw new AppException("Credenciais inválidas.");

        var (token, expiresIn) = _jwt.Generate(usuario.Id, usuario.Email, usuario.Role.ToString());
        return new TokenResponse(token, "Bearer", expiresIn);
    }
}
