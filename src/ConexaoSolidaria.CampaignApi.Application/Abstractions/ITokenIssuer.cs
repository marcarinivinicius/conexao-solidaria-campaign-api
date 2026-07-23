namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

// Composicao de IJwtTokenGenerator + IRefreshTokenGenerator + IRefreshTokenRepository -
// emite o par (access token + refresh token) usado por Login, Registro de Doador
// e Refresh, evitando repetir a mesma logica de emissao nos 3 handlers.
// Adiciona o RefreshToken ao repositorio mas NAO commita - quem chama decide
// quando dar SalvarAlteracoesAsync (mesmo padrao de unit of work do resto do app).
public interface ITokenIssuer
{
    Task<AuthTokens> EmitirAsync(Guid usuarioId, string email, string role, CancellationToken cancellationToken);
}

public record AuthTokens(string AccessToken, string RefreshToken, Guid RefreshTokenId, string Role);
