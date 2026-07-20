using ConexaoSolidaria.CampaignApi.Application.IntegrationEvents;

namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IDoacaoEventPublisher
{
    Task PublicarAsync(DoacaoRecebidaEvent evento, CancellationToken cancellationToken);
}
