using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = ConexaoSolidaria.CampaignApi.Domain.Entities.RefreshToken;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Security;

public class TokenIssuer(
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<RefreshTokenOptions> options,
    TimeProvider timeProvider) : ITokenIssuer
{
    private readonly RefreshTokenOptions _options = options.Value;

    public async Task<AuthTokens> EmitirAsync(Guid usuarioId, string email, string role, CancellationToken cancellationToken)
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;
        var (rawToken, tokenHash) = refreshTokenGenerator.Gerar();

        var refreshToken = RefreshTokenEntity.Criar(
            usuarioId, role, email, tokenHash, agora, TimeSpan.FromDays(_options.ExpirationDays));

        await refreshTokenRepository.AdicionarAsync(refreshToken, cancellationToken);

        var accessToken = jwtTokenGenerator.GerarToken(usuarioId, email, role);

        return new AuthTokens(accessToken, rawToken, refreshToken.Id, role);
    }
}
