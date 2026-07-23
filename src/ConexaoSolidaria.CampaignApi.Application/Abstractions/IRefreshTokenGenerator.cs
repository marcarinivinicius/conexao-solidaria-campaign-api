namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IRefreshTokenGenerator
{
    (string RawToken, string TokenHash) Gerar();

    string Hash(string rawToken);
}
