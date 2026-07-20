using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.ListarCampanhasAtivas;

public record ListarCampanhasAtivasQuery : IRequest<IReadOnlyList<CampanhaAtivaResult>>;

public record CampanhaAtivaResult(Guid Id, string Titulo, decimal MetaFinanceira, decimal ValorTotalArrecadado);

public class ListarCampanhasAtivasHandler(ICampanhaRepository repository)
    : IRequestHandler<ListarCampanhasAtivasQuery, IReadOnlyList<CampanhaAtivaResult>>
{
    public async Task<IReadOnlyList<CampanhaAtivaResult>> Handle(
        ListarCampanhasAtivasQuery request, CancellationToken cancellationToken)
    {
        var campanhas = await repository.ListarAtivasAsync(cancellationToken);

        return campanhas
            .Select(c => new CampanhaAtivaResult(c.Id, c.Titulo, c.MetaFinanceira, c.ValorTotalArrecadado))
            .ToList();
    }
}
