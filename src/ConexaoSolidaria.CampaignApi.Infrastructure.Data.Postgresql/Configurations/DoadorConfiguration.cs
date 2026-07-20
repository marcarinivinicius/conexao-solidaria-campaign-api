using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Configurations;

public class DoadorConfiguration : IEntityTypeConfiguration<Doador>
{
    public void Configure(EntityTypeBuilder<Doador> builder)
    {
        builder.ToTable("doadores");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Email).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Cpf).HasMaxLength(11).IsRequired();
        builder.Property(d => d.SenhaHash).IsRequired();

        builder.HasIndex(d => d.Email).IsUnique();
        builder.HasIndex(d => d.Cpf).IsUnique();
    }
}
