using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Repositories;

public class CampanhaRepository(CampaignApiDbContext dbContext) : ICampanhaRepository
{
    public Task<Campanha?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Campanhas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Campanha>> ListarAtivasAsync(CancellationToken cancellationToken) =>
        await dbContext.Campanhas
            .Where(c => c.Status == StatusCampanha.Ativa)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Campanha campanha, CancellationToken cancellationToken) =>
        await dbContext.Campanhas.AddAsync(campanha, cancellationToken);
}
