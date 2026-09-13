using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Auth;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthUseCases _uc;
    public AuthController(AuthUseCases uc) => _uc = uc;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest req, CancellationToken ct)
        => Ok(await _uc.LoginAsync(req, ct));

    [HttpPost("usuarios")]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<UsuarioResponse>> Criar(CriarUsuarioRequest req, CancellationToken ct)
    {
        var u = await _uc.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Criar), new { id = u.Id }, u);
    }

    /// <summary>
    /// Bootstrap: cria o primeiro Gerente caso não exista nenhum usuário ainda.
    /// </summary>
    [HttpPost("bootstrap")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioResponse>> Bootstrap(
        CriarUsuarioRequest req,
        [FromServices] Domain.Identidade.IUsuarioRepository repo,
        CancellationToken ct)
    {
        var any = await repo.ObterPorEmailAsync(req.Email.Trim().ToLowerInvariant(), ct);
        if (any is not null) return Conflict("Bootstrap já consumido ou e-mail em uso.");
        var u = await _uc.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Bootstrap), new { id = u.Id }, u);
    }
}
