using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;
using Microsoft.Extensions.Options;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Security;

public class SuperAdminAuthenticator(IOptions<SuperAdminCredentialsOptions> options, IPasswordHasher passwordHasher)
    : ISuperAdminAuthenticator
{
    public bool Autenticar(string email, string senha)
    {
        var config = options.Value;

        return !string.IsNullOrEmpty(config.SenhaHash)
            && string.Equals(email, config.Email, StringComparison.OrdinalIgnoreCase)
            && passwordHasher.Verificar(senha, config.SenhaHash);
    }
}
