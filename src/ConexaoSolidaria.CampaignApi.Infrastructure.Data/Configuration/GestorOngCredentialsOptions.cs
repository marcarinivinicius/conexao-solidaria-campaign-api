namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;

// Credencial unica do gestor da ONG, seedada via appsettings/env var - nao ha
// tela de "criar gestor" no MVP (fora do escopo do desafio, que so pede as
// duas roles existindo e o RBAC funcionando).
public class GestorOngCredentialsOptions
{
    public const string SectionName = "GestorOng";

    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
}
