using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;

namespace ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;

public class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly List<RefreshToken> _tokens = [];

    public IReadOnlyList<RefreshToken> Tokens => _tokens;

    public Task<RefreshToken?> ObterPorTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(_tokens.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task AdicionarAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        _tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task RevogarAtivosAsync(Guid usuarioId, string role, DateTime agora, CancellationToken cancellationToken)
    {
        foreach (var token in _tokens.Where(t => t.UsuarioId == usuarioId && t.Role == role && t.RevogadoEm is null))
            token.Revogar(agora);

        return Task.CompletedTask;
    }
}
