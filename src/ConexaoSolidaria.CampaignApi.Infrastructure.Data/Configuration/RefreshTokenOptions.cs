namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpirationDays { get; set; } = 7;
}
