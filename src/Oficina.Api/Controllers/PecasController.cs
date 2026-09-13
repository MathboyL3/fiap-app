using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Pecas;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Roles = "Atendente,Gerente,Mecanico")]
[Route("api/pecas")]
public class PecasController : ControllerBase
{
    private readonly PecaUseCases _uc;
    public PecasController(PecaUseCases uc) => _uc = uc;

    [HttpPost]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<PecaResponse>> Cadastrar(CadastrarPecaRequest req, CancellationToken ct)
    {
        var p = await _uc.CadastrarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = p.Id }, p);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PecaResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await _uc.ObterAsync(id, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PecaResponse>>> Listar([FromQuery] bool somenteAtivas = true, CancellationToken ct = default)
        => Ok(await _uc.ListarAsync(somenteAtivas, ct));

    [HttpPatch("{id:guid}/valor")]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<PecaResponse>> AtualizarValor(Guid id, AtualizarValorPecaRequest req, CancellationToken ct)
        => Ok(await _uc.AtualizarValorAsync(id, req, ct));

    [HttpPost("{id:guid}/reabastecer")]
    [Authorize(Roles = "Gerente,Atendente")]
    public async Task<ActionResult<PecaResponse>> Reabastecer(Guid id, ReabastecerEstoqueRequest req, CancellationToken ct)
        => Ok(await _uc.ReabastecerAsync(id, req, ct));

    [HttpPost("{id:guid}/inativar")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        await _uc.InativarAsync(id, ct);
        return NoContent();
    }
}
