using ConexaoSolidaria.CampaignApi.Domain.Entities;

namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AdicionarAsync(RefreshToken token, CancellationToken cancellationToken);
    Task RevogarAtivosAsync(Guid usuarioId, string role, DateTime agora, CancellationToken cancellationToken);
}
