using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Clientes;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Roles = "Atendente,Gerente")]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly ClienteUseCases _uc;
    public ClientesController(ClienteUseCases uc) => _uc = uc;

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(CriarClienteRequest req, CancellationToken ct)
    {
        var c = await _uc.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = c.Id }, c);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await _uc.ObterAsync(id, ct));

    [HttpGet("por-documento/{documento}")]
    public async Task<ActionResult<ClienteResponse>> PorDocumento(string documento, CancellationToken ct)
    {
        var c = await _uc.BuscarPorDocumentoAsync(documento, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> Listar([FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _uc.ListarAsync(Math.Max(0, skip), Math.Clamp(take, 1, 100), ct));

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> Atualizar(Guid id, AtualizarClienteRequest req, CancellationToken ct)
        => Ok(await _uc.AtualizarAsync(id, req, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await _uc.RemoverAsync(id, ct);
        return NoContent();
    }
}
