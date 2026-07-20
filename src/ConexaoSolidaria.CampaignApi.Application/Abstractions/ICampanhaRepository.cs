using ConexaoSolidaria.CampaignApi.Domain.Entities;

namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface ICampanhaRepository
{
    Task<Campanha?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Campanha>> ListarAtivasAsync(CancellationToken cancellationToken);
    Task AdicionarAsync(Campanha campanha, CancellationToken cancellationToken);
}
