using Microsoft.EntityFrameworkCore;
using Oficina.Domain.Catalogo;
using Oficina.Domain.Clientes;
using Oficina.Domain.Estoque;
using Oficina.Domain.Identidade;
using Oficina.Domain.OrdensServico;
using Oficina.Domain.Veiculos;

namespace Oficina.Infrastructure.Persistence;

public class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<EstoquePeca> Estoques => Set<EstoquePeca>();
    public DbSet<MovimentacaoEstoque> Movimentacoes => Set<MovimentacaoEstoque>();
    public DbSet<OrdemDeServico> OrdensServico => Set<OrdemDeServico>();
    public DbSet<ItemServico> ItensServico => Set<ItemServico>();
    public DbSet<ItemPeca> ItensPeca => Set<ItemPeca>();
    public DbSet<HistoricoStatus> HistoricoStatus => Set<HistoricoStatus>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(OficinaDbContext).Assembly);
    }
}
