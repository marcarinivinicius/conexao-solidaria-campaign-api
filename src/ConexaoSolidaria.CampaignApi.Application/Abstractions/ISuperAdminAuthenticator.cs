namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

// SuperAdmin nao e uma entidade no banco - e uma credencial unica seedada
// via configuracao (ver conexao-solidaria-campaign-api/appsettings.json,
// secao SuperAdmin), usada so pra cadastrar os Gestores da ONG.
public interface ISuperAdminAuthenticator
{
    bool Autenticar(string email, string senha);
}
