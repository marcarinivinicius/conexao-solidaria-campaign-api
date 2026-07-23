using ConexaoSolidaria.CampaignApi.Application.Abstractions;

namespace ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
