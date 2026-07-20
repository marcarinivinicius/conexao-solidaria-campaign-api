namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "conexao-solidaria";
    public string Audience { get; set; } = "conexao-solidaria";
    public int ExpirationMinutes { get; set; } = 60;
}
