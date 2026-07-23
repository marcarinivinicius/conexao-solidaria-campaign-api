using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Repositories;

public class GestorRepository(CampaignApiDbContext dbContext) : IGestorRepository
{
    public Task<Gestor?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Gestores.FirstOrDefaultAsync(g => g.Email == email, cancellationToken);

    public Task<bool> ExisteComEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Gestores.AnyAsync(g => g.Email == email, cancellationToken);

    public async Task AdicionarAsync(Gestor gestor, CancellationToken cancellationToken) =>
        await dbContext.Gestores.AddAsync(gestor, cancellationToken);
}
