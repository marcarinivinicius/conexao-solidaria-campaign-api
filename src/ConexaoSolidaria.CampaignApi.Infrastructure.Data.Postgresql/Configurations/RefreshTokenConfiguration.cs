using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Role).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Email).HasMaxLength(200).IsRequired();
        builder.Property(r => r.TokenHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(r => r.TokenHash).IsUnique();
        builder.HasIndex(r => new { r.UsuarioId, r.Role });

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(r => r.SubstituidoPorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
