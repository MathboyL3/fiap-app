using Oficina.Domain.Common;

namespace Oficina.Domain.OrdensServico;

public sealed class HistoricoStatus : Entity
{
    public Guid OrdemDeServicoId { get; private set; }
    public StatusOS De { get; private set; }
    public StatusOS Para { get; private set; }
    public DateTime OcorreuEm { get; private set; }
    public string? Observacao { get; private set; }

    private HistoricoStatus() { }

    internal static HistoricoStatus Criar(Guid osId, StatusOS de, StatusOS para, string? observacao)
    {
        return new HistoricoStatus
        {
            OrdemDeServicoId = osId,
            De = de,
            Para = para,
            OcorreuEm = DateTime.UtcNow,
            Observacao = observacao
        };
    }
}
