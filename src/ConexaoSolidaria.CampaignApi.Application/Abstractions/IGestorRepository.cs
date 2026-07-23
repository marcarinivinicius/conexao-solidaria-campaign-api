using ConexaoSolidaria.CampaignApi.Domain.Entities;

namespace ConexaoSolidaria.CampaignApi.Application.Abstractions;

public interface IGestorRepository
{
    Task<Gestor?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExisteComEmailAsync(string email, CancellationToken cancellationToken);
    Task AdicionarAsync(Gestor gestor, CancellationToken cancellationToken);
}
