using ConexaoSolidaria.CampaignApi.Application.Abstractions;

namespace ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;

// Hash "burro" mas determinístico e suficiente pros testes: cada token cru
// gerado e unico (contador), e o hash e so um prefixo pra facilitar debug.
public class FakeRefreshTokenGenerator : IRefreshTokenGenerator
{
    private int _contador;

    public (string RawToken, string TokenHash) Gerar()
    {
        var rawToken = $"raw-token-{++_contador}";
        return (rawToken, Hash(rawToken));
    }

    public string Hash(string rawToken) => $"hash-{rawToken}";
}
