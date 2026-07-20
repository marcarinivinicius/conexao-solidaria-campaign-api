namespace ConexaoSolidaria.CampaignApi.Application.IntegrationEvents;

public record DoacaoRecebidaEvent(
    Guid DoacaoId,
    Guid CampanhaId,
    Guid DoadorId,
    decimal Valor,
    DateTime RecebidaEm);
