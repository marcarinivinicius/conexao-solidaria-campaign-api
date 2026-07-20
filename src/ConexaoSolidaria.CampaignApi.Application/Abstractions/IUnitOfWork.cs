namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IUnitOfWork
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
