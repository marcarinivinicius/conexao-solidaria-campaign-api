using ConexaoSolidaria.CampaignApi.Application.Abstractions;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql;

public class UnitOfWork(CampaignApiDbContext dbContext) : IUnitOfWork
{
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
