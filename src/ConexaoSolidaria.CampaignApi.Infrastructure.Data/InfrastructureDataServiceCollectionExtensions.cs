using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Messaging;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Security;
using ConexaoSolidaria.Shared.RabbitMq.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data;

public static class InfrastructureDataServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SuperAdminCredentialsOptions>(configuration.GetSection(SuperAdminCredentialsOptions.SectionName));

        services.AddRabbitMqClient<CampaignApiRabbitMqConfiguration>();
        services.AddSingleton<IDoacaoEventPublisher, RabbitMqDoacaoEventPublisher>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<ISuperAdminAuthenticator, SuperAdminAuthenticator>();

        return services;
    }
}
