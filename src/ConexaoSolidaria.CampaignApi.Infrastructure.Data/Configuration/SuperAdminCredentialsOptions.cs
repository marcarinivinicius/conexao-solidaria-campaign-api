namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;

// Credencial unica do super admin, seedada via appsettings/env var - e a
// unica identidade que pode cadastrar Gestores (nao ha auto-cadastro de
// GestorONG, so de Doador).
public class SuperAdminCredentialsOptions
{
    public const string SectionName = "SuperAdmin";

    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
}
