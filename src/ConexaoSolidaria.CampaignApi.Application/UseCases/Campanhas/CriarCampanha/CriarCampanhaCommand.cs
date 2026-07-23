using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.CriarCampanha;

public record CriarCampanhaCommand(
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal MetaFinanceira) : IRequest<CriarCampanhaResult>;

// Datas coerentes (fim > inicio) e financas > 0 tambem sao checadas na
// factory de dominio (Campanha.Criar) - aqui e so pra falhar mais cedo,
// antes de bater no banco/domain.
public class CriarCampanhaCommandValidator : AbstractValidator<CriarCampanhaCommand>
{
    public CriarCampanhaCommandValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descricao).NotEmpty();
        RuleFor(x => x.DataFim).GreaterThan(x => x.DataInicio);
        RuleFor(x => x.MetaFinanceira).GreaterThan(0);
    }
}

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
