using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Repositories;

public class DoadorRepository(CampaignApiDbContext dbContext) : IDoadorRepository
{
    public Task<Doador?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Doadores.FirstOrDefaultAsync(d => d.Email == email, cancellationToken);

    public Task<bool> ExisteComEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Doadores.AnyAsync(d => d.Email == email, cancellationToken);

    public async Task AdicionarAsync(Doador doador, CancellationToken cancellationToken) =>
        await dbContext.Doadores.AddAsync(doador, cancellationToken);
}
