using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oficina.Domain.Clientes;
using Oficina.Domain.ValueObjects;

namespace Oficina.Infrastructure.Persistence.Configurations;

public class ClienteConfig : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("clientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200).IsRequired();
        b.Property(x => x.Telefone).HasMaxLength(30).IsRequired();
        b.Property(x => x.CriadoEm).IsRequired();

        b.OwnsOne(x => x.Documento, doc =>
        {
            doc.Property(d => d.Tipo).HasColumnName("documento_tipo").HasConversion<string>().HasMaxLength(10).IsRequired();
            doc.Property(d => d.Numero).HasColumnName("documento_numero").HasMaxLength(14).IsRequired();
            doc.HasIndex(d => d.Numero).IsUnique();
        });
    }
}
