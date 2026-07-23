namespace ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;

public class FakeTimeProvider(DateTimeOffset agora) : TimeProvider
{
    public DateTimeOffset Agora { get; set; } = agora;

    public override DateTimeOffset GetUtcNow() => Agora;
}
