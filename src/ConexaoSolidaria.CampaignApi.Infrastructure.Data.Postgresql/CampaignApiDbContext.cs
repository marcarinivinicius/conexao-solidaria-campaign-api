using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql;

public class CampaignApiDbContext(DbContextOptions<CampaignApiDbContext> options) : DbContext(options)
{
    public DbSet<Campanha> Campanhas => Set<Campanha>();
    public DbSet<Doador> Doadores => Set<Doador>();
    public DbSet<Gestor> Gestores => Set<Gestor>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DoacaoProcessada> DoacoesProcessadas => Set<DoacaoProcessada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CampaignApiDbContext).Assembly);
    }
}
