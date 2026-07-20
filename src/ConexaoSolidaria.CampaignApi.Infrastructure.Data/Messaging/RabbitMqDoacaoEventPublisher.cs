using System.Text;
using System.Text.Json;
using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Application.IntegrationEvents;
using ConexaoSolidaria.Shared.RabbitMq;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Messaging;

public class RabbitMqDoacaoEventPublisher(IRabbitMqClientPublisher<CampaignApiRabbitMqConfiguration> publisher)
    : IDoacaoEventPublisher
{
    public async Task PublicarAsync(DoacaoRecebidaEvent evento, CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evento));

        var publicado = await publisher.Publish(
            exchange: MessagingConstants.DoacoesExchange,
            body: body,
            routingKey: MessagingConstants.DoacaoRecebidaRoutingKey,
            cancellationToken: cancellationToken);

        if (!publicado)
            throw new InvalidOperationException("Falha ao publicar DoacaoRecebidaEvent no RabbitMQ.");
    }
}
