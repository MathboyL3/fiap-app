using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Servicos;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Roles = "Atendente,Gerente,Mecanico")]
[Route("api/servicos")]
public class ServicosController : ControllerBase
{
    private readonly ServicoUseCases _uc;
    public ServicosController(ServicoUseCases uc) => _uc = uc;

    [HttpPost]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<ServicoResponse>> Cadastrar(CadastrarServicoRequest req, CancellationToken ct)
    {
        var s = await _uc.CadastrarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = s.Id }, s);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServicoResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await _uc.ObterAsync(id, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServicoResponse>>> Listar([FromQuery] bool somenteAtivos = true, CancellationToken ct = default)
        => Ok(await _uc.ListarAsync(somenteAtivos, ct));

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<ServicoResponse>> Atualizar(Guid id, AtualizarServicoRequest req, CancellationToken ct)
        => Ok(await _uc.AtualizarAsync(id, req, ct));

    [HttpPost("{id:guid}/inativar")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        await _uc.InativarAsync(id, ct);
        return NoContent();
    }
}
