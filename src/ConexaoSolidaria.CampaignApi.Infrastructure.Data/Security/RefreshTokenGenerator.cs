using System.Security.Cryptography;
using ConexaoSolidaria.CampaignApi.Application.Abstractions;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Security;

// Refresh token e um segredo aleatorio opaco, nao um JWT - alta entropia
// (256 bits) ja e suficiente contra forca bruta, entao usa hash rapido
// (SHA-256) pra permitir busca por igualdade no banco, diferente do
// BCrypt de senha (que precisa ser lento de proposito).
public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public (string RawToken, string TokenHash) Gerar()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (rawToken, Hash(rawToken));
    }

    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
