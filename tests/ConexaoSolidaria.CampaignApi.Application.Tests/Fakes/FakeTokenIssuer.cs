using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;

namespace ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;

// Espelha o TokenIssuer real (Infrastructure.Data) o suficiente pra testar
// os handlers em isolamento: gera um refresh token via IRefreshTokenGenerator,
// persiste (sem commitar) via IRefreshTokenRepository, e devolve um "JWT" falso.
public class FakeTokenIssuer(
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    TimeProvider timeProvider) : ITokenIssuer
{
    public TimeSpan RefreshTokenTempoDeVida { get; set; } = TimeSpan.FromDays(7);

    public async Task<AuthTokens> EmitirAsync(Guid usuarioId, string email, string role, CancellationToken cancellationToken)
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;
        var (rawToken, tokenHash) = refreshTokenGenerator.Gerar();

        var refreshToken = RefreshToken.Criar(usuarioId, role, email, tokenHash, agora, RefreshTokenTempoDeVida);
        await refreshTokenRepository.AdicionarAsync(refreshToken, cancellationToken);

        return new AuthTokens($"fake-jwt-{usuarioId}", rawToken, refreshToken.Id, role);
    }
}
