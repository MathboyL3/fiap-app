using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.DTOs;
using Oficina.Application.UseCases.OrdensServico;
using Oficina.Domain.OrdensServico;

namespace Oficina.Api.Controllers;

[ApiController]
[Authorize(Roles = "Atendente,Gerente,Mecanico")]
[Route("api/ordens-servico")]
public class OrdensServicoController : ControllerBase
{
    private readonly OrdemServicoUseCases _uc;
    private readonly IConfiguration _config;
    public OrdensServicoController(OrdemServicoUseCases uc, IConfiguration config)
    {
        _uc = uc;
        _config = config;
    }

    [HttpPost]
    public async Task<ActionResult<OSResponse>> Abrir(AbrirOSRequest req, CancellationToken ct)
    {
        var os = await _uc.AbrirAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = os.Id }, os);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OSResponse>> Obter(Guid id, CancellationToken ct)
        => Ok(await _uc.ObterAsync(id, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OSResumoResponse>>> Listar(
        [FromQuery] int skip = 0, [FromQuery] int take = 20, [FromQuery] StatusOS? status = null, CancellationToken ct = default)
        => Ok(await _uc.ListarAsync(Math.Max(0, skip), Math.Clamp(take, 1, 100), status, ct));

    /// <summary>
    /// Painel de acompanhamento (Fase 2): OS ativas ordenadas por prioridade de status
    /// (Em execução > Aguardando aprovação > Em diagnóstico > Recebida), mais antigas primeiro.
    /// Não inclui OS Finalizadas, Entregues ou Canceladas.
    /// </summary>
    [HttpGet("painel")]
    public async Task<ActionResult<IReadOnlyList<OSResumoResponse>>> Painel(
        [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _uc.ListarPainelAsync(Math.Max(0, skip), Math.Clamp(take, 1, 100), ct));

    [HttpPost("{id:guid}/servicos")]
    public async Task<ActionResult<OSResponse>> IncluirServico(Guid id, IncluirItemServicoRequest req, CancellationToken ct)
        => Ok(await _uc.IncluirServicoAsync(id, req, ct));

    [HttpPost("{id:guid}/pecas")]
    public async Task<ActionResult<OSResponse>> IncluirPeca(Guid id, IncluirItemPecaRequest req, CancellationToken ct)
        => Ok(await _uc.IncluirPecaAsync(id, req, ct));

    [HttpDelete("{id:guid}/pecas/{itemId:guid}")]
    public async Task<ActionResult<OSResponse>> RemoverPeca(Guid id, Guid itemId, CancellationToken ct)
        => Ok(await _uc.RemoverPecaAsync(id, itemId, ct));

    [HttpPost("{id:guid}/diagnostico")]
    [Authorize(Roles = "Mecanico,Gerente")]
    public async Task<ActionResult<OSResponse>> IniciarDiagnostico(Guid id, IniciarDiagnosticoRequest req, CancellationToken ct)
        => Ok(await _uc.IniciarDiagnosticoAsync(id, req, ct));

    [HttpPost("{id:guid}/enviar-aprovacao")]
    public async Task<ActionResult<OSResponse>> EnviarAprovacao(Guid id, CancellationToken ct)
        => Ok(await _uc.EnviarParaAprovacaoAsync(id, ct));

    [HttpPost("{id:guid}/aprovar")]
    public async Task<ActionResult<OSResponse>> Aprovar(Guid id, CancellationToken ct)
        => Ok(await _uc.AprovarOrcamentoAsync(id, ct));

    [HttpPost("{id:guid}/rejeitar")]
    public async Task<ActionResult<OSResponse>> Rejeitar(Guid id, RejeitarOrcamentoRequest req, CancellationToken ct)
        => Ok(await _uc.RejeitarOrcamentoAsync(id, req, ct));

    [HttpPost("{id:guid}/finalizar")]
    [Authorize(Roles = "Mecanico,Gerente")]
    public async Task<ActionResult<OSResponse>> Finalizar(Guid id, CancellationToken ct)
        => Ok(await _uc.FinalizarAsync(id, ct));

    [HttpPost("{id:guid}/entregar")]
    public async Task<ActionResult<OSResponse>> Entregar(Guid id, CancellationToken ct)
        => Ok(await _uc.EntregarAsync(id, ct));

    [HttpPost("{id:guid}/cancelar")]
    public async Task<ActionResult<OSResponse>> Cancelar(Guid id, CancelarOSRequest req, CancellationToken ct)
        => Ok(await _uc.CancelarAsync(id, req, ct));

    [HttpGet("metricas/tempo-medio")]
    [Authorize(Roles = "Gerente")]
    public async Task<ActionResult<TempoMedioExecucaoResponse>> TempoMedio(
        [FromQuery] DateTime? de = null, [FromQuery] DateTime? ate = null, CancellationToken ct = default)
        => Ok(await _uc.CalcularTempoMedioAsync(de, ate, ct));

    /// <summary>
    /// Endpoint público (sem JWT) para o cliente consultar status da OS.
    /// </summary>
    [HttpGet("publico/{id:guid}/status")]
    [AllowAnonymous]
    public async Task<ActionResult<StatusPublicoResponse>> StatusPublico(Guid id, CancellationToken ct)
        => Ok(await _uc.ObterStatusPublicoAsync(id, ct));

    /// <summary>
    /// Webhook público (sem JWT) para aprovação/rejeição de orçamento por sistema externo (Fase 2).
    /// Protegido por segredo compartilhado no header <c>X-Webhook-Secret</c>.
    /// </summary>
    [HttpPost("webhook/aprovacao")]
    [AllowAnonymous]
    public async Task<ActionResult<OSResponse>> WebhookAprovacao(
        [FromBody] WebhookAprovacaoRequest req,
        [FromHeader(Name = "X-Webhook-Secret")] string? secret,
        CancellationToken ct)
    {
        var expected = _config["Webhook:Secret"];
        if (string.IsNullOrEmpty(expected) || secret != expected)
            return Unauthorized(new { error = "Segredo de webhook inválido." });

        return Ok(await _uc.ProcessarWebhookAprovacaoAsync(req, ct));
    }
}
