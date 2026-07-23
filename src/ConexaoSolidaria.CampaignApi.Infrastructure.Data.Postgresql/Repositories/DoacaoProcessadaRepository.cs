using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Repositories;

public class DoacaoProcessadaRepository(CampaignApiDbContext dbContext) : IDoacaoProcessadaRepository
{
    public async Task<bool> TentarRegistrarAsync(Guid doacaoId, Guid campanhaId, Guid doadorId, decimal valor, DateTime agora, CancellationToken cancellationToken)
    {
        var linhasAfetadas = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO doacoes_processadas ("Id", "CampanhaId", "DoadorId", "Valor", "CriadoEm")
            VALUES ({doacaoId}, {campanhaId}, {doadorId}, {valor}, {agora})
            ON CONFLICT ("Id") DO NOTHING
            """,
            cancellationToken);

        return linhasAfetadas > 0;
    }
}
