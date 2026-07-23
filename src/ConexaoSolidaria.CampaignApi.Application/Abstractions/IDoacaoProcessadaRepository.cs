namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IDoacaoProcessadaRepository
{
    // Insere de forma atomica (INSERT ... ON CONFLICT DO NOTHING) - retorna
    // true se inseriu (primeira vez), false se o DoacaoId ja existia
    // (retry do cliente com a mesma Idempotency-Key).
    Task<bool> TentarRegistrarAsync(Guid doacaoId, Guid campanhaId, Guid doadorId, decimal valor, DateTime agora, CancellationToken cancellationToken);
}
