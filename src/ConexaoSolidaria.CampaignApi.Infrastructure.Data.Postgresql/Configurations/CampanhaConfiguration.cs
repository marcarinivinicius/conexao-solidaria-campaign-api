using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Configurations;

public class CampanhaConfiguration : IEntityTypeConfiguration<Campanha>
{
    public void Configure(EntityTypeBuilder<Campanha> builder)
    {
        builder.ToTable("campanhas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(4000).IsRequired();
        builder.Property(c => c.MetaFinanceira).HasColumnType("numeric(18,2)");
        builder.Property(c => c.ValorTotalArrecadado).HasColumnType("numeric(18,2)");
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Status);
    }
}
