using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Repositories;

public class RefreshTokenRepository(CampaignApiDbContext dbContext) : IRefreshTokenRepository
{
    public Task<RefreshToken?> ObterPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

    public async Task AdicionarAsync(RefreshToken token, CancellationToken cancellationToken) =>
        await dbContext.RefreshTokens.AddAsync(token, cancellationToken);

    public async Task RevogarAtivosAsync(Guid usuarioId, string role, DateTime agora, CancellationToken cancellationToken)
    {
        var ativos = await dbContext.RefreshTokens
            .Where(r => r.UsuarioId == usuarioId && r.Role == role && r.RevogadoEm == null)
            .ToListAsync(cancellationToken);

        foreach (var token in ativos)
            token.Revogar(agora);
    }
}
