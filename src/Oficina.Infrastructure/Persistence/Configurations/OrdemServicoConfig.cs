using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.OrdensServico;

namespace Oficina.Infrastructure.Persistence.Configurations;

public class OrdemServicoConfig : IEntityTypeConfiguration<OrdemDeServico>
{
    public void Configure(EntityTypeBuilder<OrdemDeServico> b)
    {
        b.ToTable("ordens_servico");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.ClienteId).IsRequired();
        b.Property(x => x.VeiculoId).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.CriadaEm).IsRequired();
        b.Property(x => x.IniciadaExecucaoEm);
        b.Property(x => x.FinalizadaEm);
        b.Property(x => x.EntregueEm);
        b.Property(x => x.ObservacoesDiagnostico).HasMaxLength(2000);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => x.Status);

        b.Ignore(x => x.DomainEvents);
        b.Ignore(x => x.TempoExecucao);

        b.OwnsOne(x => x.ValorTotal, m =>
        {
            m.Property(p => p.Amount).HasColumnName("valor_total_amount").HasColumnType("numeric(18,2)").IsRequired();
            m.Property(p => p.Currency).HasColumnName("valor_total_currency").HasMaxLength(3).IsRequired();
        });

        // Relações HasMany — entidades reais com FK explícita
        b.HasMany(x => x.Servicos)
            .WithOne()
            .HasForeignKey(s => s.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Pecas)
            .WithOne()
            .HasForeignKey(s => s.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Historico)
            .WithOne()
            .HasForeignKey(h => h.OrdemDeServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Garante uso do backing field (lista privada _servicos, _pecas, _historico)
        b.Navigation(x => x.Servicos).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Pecas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Historico).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ItemServicoConfig : IEntityTypeConfiguration<ItemServico>
{
    public void Configure(EntityTypeBuilder<ItemServico> b)
    {
        b.ToTable("os_itens_servico");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.OrdemDeServicoId).IsRequired();
        b.Property(x => x.ServicoId).IsRequired();
        b.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantidade).IsRequired();
        b.Ignore(x => x.Subtotal);
        b.OwnsOne(x => x.ValorUnitario, m =>
        {
            m.Property(v => v.Amount).HasColumnName("valor_unitario_amount").HasColumnType("numeric(18,2)").IsRequired();
            m.Property(v => v.Currency).HasColumnName("valor_unitario_currency").HasMaxLength(3).IsRequired();
        });
    }
}

public class ItemPecaConfig : IEntityTypeConfiguration<ItemPeca>
{
    public void Configure(EntityTypeBuilder<ItemPeca> b)
    {
        b.ToTable("os_itens_peca");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.OrdemDeServicoId).IsRequired();
        b.Property(x => x.PecaId).IsRequired();
        b.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantidade).IsRequired();
        b.Ignore(x => x.Subtotal);
        b.OwnsOne(x => x.ValorUnitario, m =>
        {
            m.Property(v => v.Amount).HasColumnName("valor_unitario_amount").HasColumnType("numeric(18,2)").IsRequired();
            m.Property(v => v.Currency).HasColumnName("valor_unitario_currency").HasMaxLength(3).IsRequired();
        });
    }
}

public class HistoricoStatusConfig : IEntityTypeConfiguration<HistoricoStatus>
{
    public void Configure(EntityTypeBuilder<HistoricoStatus> b)
    {
        b.ToTable("os_historico_status");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.OrdemDeServicoId).IsRequired();
        b.Property(x => x.De).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.Para).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.OcorreuEm).IsRequired();
        b.Property(x => x.Observacao).HasMaxLength(500);
    }
}
