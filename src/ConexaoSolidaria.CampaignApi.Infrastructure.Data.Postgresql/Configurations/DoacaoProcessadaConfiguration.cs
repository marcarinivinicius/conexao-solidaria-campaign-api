using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Configurations;

public class DoacaoProcessadaConfiguration : IEntityTypeConfiguration<DoacaoProcessada>
{
    public void Configure(EntityTypeBuilder<DoacaoProcessada> builder)
    {
        builder.ToTable("doacoes_processadas");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
    }
}
