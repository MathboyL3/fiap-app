using Oficina.Domain.Common;
using Oficina.Domain.ValueObjects;

namespace Oficina.Domain.OrdensServico;

public sealed class OrdemDeServico : AggregateRoot
{
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public StatusOS Status { get; private set; }
    public Money ValorTotal { get; private set; } = Money.Zero;
    public DateTime CriadaEm { get; private set; }
    public DateTime? IniciadaExecucaoEm { get; private set; }
    public DateTime? FinalizadaEm { get; private set; }
    public DateTime? EntregueEm { get; private set; }
    public string? ObservacoesDiagnostico { get; private set; }

    private readonly List<ItemServico> _servicos = new();
    private readonly List<ItemPeca> _pecas = new();
    private readonly List<HistoricoStatus> _historico = new();

    public IReadOnlyCollection<ItemServico> Servicos => _servicos.AsReadOnly();
    public IReadOnlyCollection<ItemPeca> Pecas => _pecas.AsReadOnly();
    public IReadOnlyCollection<HistoricoStatus> Historico => _historico.AsReadOnly();

    public TimeSpan? TempoExecucao =>
        IniciadaExecucaoEm.HasValue && FinalizadaEm.HasValue
            ? FinalizadaEm.Value - IniciadaExecucaoEm.Value
            : null;

    private OrdemDeServico() { }

    public static OrdemDeServico Abrir(Guid clienteId, Guid veiculoId)
    {
        if (clienteId == Guid.Empty) throw new DomainException("Cliente é obrigatório.");
        if (veiculoId == Guid.Empty) throw new DomainException("Veículo é obrigatório.");

        var os = new OrdemDeServico
        {
            ClienteId = clienteId,
            VeiculoId = veiculoId,
            Status = StatusOS.Recebida,
            CriadaEm = DateTime.UtcNow
        };
        os._historico.Add(HistoricoStatus.Criar(os.Id, StatusOS.Recebida, StatusOS.Recebida, "OS aberta."));
        return os;
    }

    public void IncluirServico(Guid servicoId, string descricao, Money valorUnitario, int quantidade)
    {
        GarantirEditavel("incluir serviço");
        var item = ItemServico.Criar(Id, servicoId, descricao, valorUnitario, quantidade);
        _servicos.Add(item);
        Recalcular();
    }

    public void RemoverServico(Guid itemServicoId)
    {
        GarantirEditavel("remover serviço");
        var item = _servicos.SingleOrDefault(s => s.Id == itemServicoId)
            ?? throw new DomainException("Item de serviço não encontrado na OS.");
        _servicos.Remove(item);
        Recalcular();
    }

    public void IncluirPeca(Guid pecaId, string descricao, Money valorUnitario, int quantidade)
    {
        GarantirEditavel("incluir peça");
        var item = ItemPeca.Criar(Id, pecaId, descricao, valorUnitario, quantidade);
        _pecas.Add(item);
        Recalcular();
    }

    public void RemoverPeca(Guid itemPecaId)
    {
        GarantirEditavel("remover peça");
        var item = _pecas.SingleOrDefault(p => p.Id == itemPecaId)
            ?? throw new DomainException("Item de peça não encontrado na OS.");
        _pecas.Remove(item);
        Recalcular();
    }

    public void IniciarDiagnostico(string? observacoes = null)
    {
        ExigirTransicao(StatusOS.Recebida, StatusOS.EmDiagnostico, observacoes);
        ObservacoesDiagnostico = observacoes;
    }

    public void EnviarParaAprovacao()
    {
        if (_servicos.Count == 0 && _pecas.Count == 0)
            throw new DomainException("OS sem itens não pode ser enviada para aprovação.");
        ExigirTransicao(StatusOS.EmDiagnostico, StatusOS.AguardandoAprovacao, "Orçamento enviado ao cliente.");
    }

    public void AprovarOrcamento()
    {
        ExigirTransicao(StatusOS.AguardandoAprovacao, StatusOS.EmExecucao, "Orçamento aprovado pelo cliente.");
        IniciadaExecucaoEm = DateTime.UtcNow;
    }

    public void RejeitarOrcamento(string motivo)
    {
        ExigirTransicao(StatusOS.AguardandoAprovacao, StatusOS.Cancelada, $"Orçamento rejeitado: {motivo}");
    }

    public void Finalizar()
    {
        ExigirTransicao(StatusOS.EmExecucao, StatusOS.Finalizada, "Execução concluída.");
        FinalizadaEm = DateTime.UtcNow;
    }

    public void Entregar()
    {
        ExigirTransicao(StatusOS.Finalizada, StatusOS.Entregue, "Veículo entregue ao cliente.");
        EntregueEm = DateTime.UtcNow;
    }

    public void Cancelar(string motivo)
    {
        if (Status is StatusOS.Entregue or StatusOS.Cancelada)
            throw new DomainException($"OS no status {Status} não pode ser cancelada.");
        var anterior = Status;
        Status = StatusOS.Cancelada;
        _historico.Add(HistoricoStatus.Criar(Id, anterior, StatusOS.Cancelada, motivo));
    }

    private void ExigirTransicao(StatusOS de, StatusOS para, string? obs)
    {
        if (Status != de)
            throw new DomainException($"Transição inválida: status atual é {Status}, esperado {de} para mudar para {para}.");
        Status = para;
        _historico.Add(HistoricoStatus.Criar(Id, de, para, obs));
    }

    private void GarantirEditavel(string acao)
    {
        if (Status is StatusOS.AguardandoAprovacao or StatusOS.EmExecucao or StatusOS.Finalizada or StatusOS.Entregue or StatusOS.Cancelada)
            throw new DomainException($"Não é possível {acao}: OS está no status {Status}.");
    }

    private void Recalcular()
    {
        var total = Money.Zero;
        foreach (var s in _servicos) total = total.Add(s.Subtotal);
        foreach (var p in _pecas) total = total.Add(p.Subtotal);
        ValorTotal = total;
    }
}
