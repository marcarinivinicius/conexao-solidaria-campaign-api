using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.CriarCampanha;

public record CriarCampanhaCommand(
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal MetaFinanceira) : IRequest<CriarCampanhaResult>;

public record CriarCampanhaResult(Guid Id);

public class CriarCampanhaHandler(
    ICampanhaRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CriarCampanhaCommand, CriarCampanhaResult>
{
    public async Task<CriarCampanhaResult> Handle(CriarCampanhaCommand request, CancellationToken cancellationToken)
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;

        var campanha = Campanha.Criar(
            request.Titulo,
            request.Descricao,
            request.DataInicio,
            request.DataFim,
            request.MetaFinanceira,
            agora);

        await repository.AdicionarAsync(campanha, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CriarCampanhaResult(campanha.Id);
    }
}
