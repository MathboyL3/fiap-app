using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Veiculos;

namespace Oficina.Infrastructure.Persistence.Configurations;

public class VeiculoConfig : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> b)
    {
        b.ToTable("veiculos");
        b.HasKey(x => x.Id);
        b.Property(x => x.ClienteId).IsRequired();
        b.Property(x => x.Marca).HasMaxLength(100).IsRequired();
        b.Property(x => x.Modelo).HasMaxLength(100).IsRequired();
        b.Property(x => x.Ano).IsRequired();
        b.HasIndex(x => x.ClienteId);
        b.OwnsOne(x => x.Placa, pl =>
        {
            pl.Property(p => p.Valor).HasColumnName("placa").HasMaxLength(7).IsRequired();
            pl.HasIndex(p => p.Valor).IsUnique();
        });
    }
}
