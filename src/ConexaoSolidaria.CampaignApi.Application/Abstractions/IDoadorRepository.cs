using ConexaoSolidaria.CampaignApi.Domain.Entities;

namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IDoadorRepository
{
    Task<Doador?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExisteComEmailAsync(string email, CancellationToken cancellationToken);
    Task AdicionarAsync(Doador doador, CancellationToken cancellationToken);
}
