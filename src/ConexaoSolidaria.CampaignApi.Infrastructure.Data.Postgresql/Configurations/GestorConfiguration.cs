using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Configurations;

public class GestorConfiguration : IEntityTypeConfiguration<Gestor>
{
    public void Configure(EntityTypeBuilder<Gestor> builder)
    {
        builder.ToTable("gestores");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(g => g.Email).HasMaxLength(200).IsRequired();
        builder.Property(g => g.SenhaHash).IsRequired();

        builder.HasIndex(g => g.Email).IsUnique();
    }
}
