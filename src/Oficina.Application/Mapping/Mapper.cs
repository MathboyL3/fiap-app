using Oficina.Application.DTOs;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Clientes;
using Oficina.Domain.Estoque;
using Oficina.Domain.Identidade;
using Oficina.Domain.OrdensServico;
using Oficina.Domain.Veiculos;

namespace Oficina.Application.Mapping;

public static class Mapper
{
    public static ClienteResponse ToResponse(this Cliente c) => new(
        c.Id, c.Nome, c.Documento.Numero, c.Documento.Tipo.ToString(),
        c.Email, c.Telefone, c.CriadoEm);

    public static VeiculoResponse ToResponse(this Veiculo v) => new(
        v.Id, v.ClienteId, v.Placa.Formatada(), v.Marca, v.Modelo, v.Ano);

    public static ServicoResponse ToResponse(this Servico s) => new(
        s.Id, s.Nome, s.Descricao, s.ValorBase.Amount, (int)s.TempoEstimado.TotalMinutes, s.Ativo);

    public static PecaResponse ToResponse(this Peca p, EstoquePeca? e) => new(
        p.Id, p.Nome, p.Codigo, p.Unidade, p.Valor.Amount, p.Ativo,
        e?.QuantidadeDisponivel ?? 0,
        e?.QuantidadeReservada ?? 0,
        e?.LimiteMinimo ?? 0,
        e?.EstoqueBaixo ?? false);

    public static ItemServicoResponse ToResponse(this ItemServico i) => new(
        i.Id, i.ServicoId, i.Descricao, i.ValorUnitario.Amount, i.Quantidade, i.Subtotal.Amount);

    public static ItemPecaResponse ToResponse(this ItemPeca i) => new(
        i.Id, i.PecaId, i.Descricao, i.ValorUnitario.Amount, i.Quantidade, i.Subtotal.Amount);

    public static HistoricoStatusResponse ToResponse(this HistoricoStatus h) => new(
        h.De.ToString(), h.Para.ToString(), h.OcorreuEm, h.Observacao);

    public static OSResponse ToResponse(this OrdemDeServico o) => new(
        o.Id, o.ClienteId, o.VeiculoId, o.Status.ToString(), o.ValorTotal.Amount,
        o.CriadaEm, o.IniciadaExecucaoEm, o.FinalizadaEm, o.EntregueEm,
        o.ObservacoesDiagnostico,
        o.Servicos.Select(ToResponse).ToList(),
        o.Pecas.Select(ToResponse).ToList(),
        o.Historico.Select(ToResponse).ToList());

    public static OSResumoResponse ToResumo(this OrdemDeServico o) => new(
        o.Id, o.ClienteId, o.VeiculoId, o.Status.ToString(), o.ValorTotal.Amount, o.CriadaEm);

    public static UsuarioResponse ToResponse(this Usuario u) => new(
        u.Id, u.Nome, u.Email, u.Role.ToString());
}
