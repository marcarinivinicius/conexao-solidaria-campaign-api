using ConexaoSolidaria.Shared.RabbitMq;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Messaging;

public class CampaignApiRabbitMqConfiguration : IRabbitMqClientConfiguration
{
    public string JsonFile => "rabbitmq-settings.json";
}
