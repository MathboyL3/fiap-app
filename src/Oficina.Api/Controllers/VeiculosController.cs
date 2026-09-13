using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.Veiculos;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Roles = "Atendente,Gerente")]
[Route("api/veiculos")]
public class VeiculosController : ControllerBase
{
    private readonly VeiculoUseCases _uc;
    public VeiculosController(VeiculoUseCases uc) => _uc = uc;

    [HttpPost]
    public async Task<ActionResult<VeiculoResponse>> Cadastrar(CadastrarVeiculoRequest req, CancellationToken ct)
    {
        var v = await _uc.CadastrarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = v.Id }, v);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VeiculoResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await _uc.ObterAsync(id, ct));

    [HttpGet("por-cliente/{clienteId:guid}")]
    public async Task<ActionResult<IReadOnlyList<VeiculoResponse>>> PorCliente(Guid clienteId, CancellationToken ct)
        => Ok(await _uc.ListarPorClienteAsync(clienteId, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Gerente")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await _uc.RemoverAsync(id, ct);
        return NoContent();
    }
}
