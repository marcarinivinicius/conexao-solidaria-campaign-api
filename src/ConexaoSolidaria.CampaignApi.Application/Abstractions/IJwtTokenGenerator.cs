namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GerarToken(Guid usuarioId, string email, string role);
}
