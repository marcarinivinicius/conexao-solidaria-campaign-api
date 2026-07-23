using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql;

public static class PostgresqlServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresqlData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres nao configurada.");

        services.AddDbContext<CampaignApiDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ICampanhaRepository, CampanhaRepository>();
        services.AddScoped<IDoadorRepository, DoadorRepository>();
        services.AddScoped<IGestorRepository, GestorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static void ApplyMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CampaignApiDbContext>();
        dbContext.Database.Migrate();
    }
}
