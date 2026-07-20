using ConexaoSolidaria.CampaignApi.Application.Abstractions;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
