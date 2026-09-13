using Oficina.Domain.Common;

namespace Oficina.Domain.Estoque;

public sealed class EstoquePeca : AggregateRoot
{
    public Guid PecaId { get; private set; }
    public int QuantidadeDisponivel { get; private set; }
    public int QuantidadeReservada { get; private set; }
    public int LimiteMinimo { get; private set; }

    private readonly List<MovimentacaoEstoque> _movimentacoes = new();
    public IReadOnlyCollection<MovimentacaoEstoque> Movimentacoes => _movimentacoes.AsReadOnly();

    public int Total => QuantidadeDisponivel + QuantidadeReservada;
    public bool EstoqueBaixo => Total <= LimiteMinimo;

    private EstoquePeca() { }

    public static EstoquePeca Criar(Guid pecaId, int quantidadeInicial = 0, int limiteMinimo = 0)
    {
        if (pecaId == Guid.Empty)
            throw new DomainException("Estoque deve estar associado a uma peça.");
        if (quantidadeInicial < 0)
            throw new DomainException("Quantidade inicial não pode ser negativa.");
        if (limiteMinimo < 0)
            throw new DomainException("Limite mínimo não pode ser negativo.");

        var estoque = new EstoquePeca
        {
            PecaId = pecaId,
            QuantidadeDisponivel = quantidadeInicial,
            QuantidadeReservada = 0,
            LimiteMinimo = limiteMinimo
        };
        if (quantidadeInicial > 0)
            estoque._movimentacoes.Add(MovimentacaoEstoque.Criar(estoque.Id, TipoMovimentacao.Entrada, quantidadeInicial, "Estoque inicial"));
        return estoque;
    }

    public void Reabastecer(int quantidade, string? referencia = null)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade de reabastecimento deve ser positiva.");
        QuantidadeDisponivel += quantidade;
        _movimentacoes.Add(MovimentacaoEstoque.Criar(Id, TipoMovimentacao.Entrada, quantidade, referencia));
    }

    public void Reservar(int quantidade, string referencia)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade reservada deve ser positiva.");
        if (QuantidadeDisponivel < quantidade)
            throw new DomainException($"Estoque insuficiente. Disponível: {QuantidadeDisponivel}, solicitado: {quantidade}.");

        QuantidadeDisponivel -= quantidade;
        QuantidadeReservada += quantidade;
        _movimentacoes.Add(MovimentacaoEstoque.Criar(Id, TipoMovimentacao.Reserva, quantidade, referencia));
    }

    public void EstornarReserva(int quantidade, string referencia)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade de estorno deve ser positiva.");
        if (QuantidadeReservada < quantidade)
            throw new DomainException($"Não há reserva suficiente para estornar. Reservado: {QuantidadeReservada}.");

        QuantidadeReservada -= quantidade;
        QuantidadeDisponivel += quantidade;
        _movimentacoes.Add(MovimentacaoEstoque.Criar(Id, TipoMovimentacao.EstornoReserva, quantidade, referencia));
    }

    public void ConsumirReservado(int quantidade, string referencia)
    {
        if (quantidade <= 0)
            throw new DomainException("Quantidade consumida deve ser positiva.");
        if (QuantidadeReservada < quantidade)
            throw new DomainException($"Não há reserva suficiente para consumir. Reservado: {QuantidadeReservada}.");

        QuantidadeReservada -= quantidade;
        _movimentacoes.Add(MovimentacaoEstoque.Criar(Id, TipoMovimentacao.ConsumoReservado, quantidade, referencia));
    }

    public void DefinirLimiteMinimo(int limite)
    {
        if (limite < 0)
            throw new DomainException("Limite mínimo não pode ser negativo.");
        LimiteMinimo = limite;
    }
}
