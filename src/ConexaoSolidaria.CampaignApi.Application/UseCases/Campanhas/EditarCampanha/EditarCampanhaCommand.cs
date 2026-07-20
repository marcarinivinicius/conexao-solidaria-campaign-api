using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Enums;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.EditarCampanha;

public record EditarCampanhaCommand(
    Guid Id,
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal MetaFinanceira,
    StatusCampanha Status) : IRequest;

public class EditarCampanhaHandler(
    ICampanhaRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<EditarCampanhaCommand>
{
    public async Task Handle(EditarCampanhaCommand request, CancellationToken cancellationToken)
    {
        var campanha = await repository.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Campanha nao encontrada.");

        var agora = timeProvider.GetUtcNow().UtcDateTime;

        campanha.Editar(
            request.Titulo,
            request.Descricao,
            request.DataInicio,
            request.DataFim,
            request.MetaFinanceira,
            request.Status,
            agora);

        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);
    }
}
