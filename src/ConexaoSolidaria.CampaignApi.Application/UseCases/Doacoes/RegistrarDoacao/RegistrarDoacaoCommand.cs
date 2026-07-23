using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Application.IntegrationEvents;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Doacoes.RegistrarDoacao;

// IdempotencyKey e opcional: se o cliente mandar (header Idempotency-Key),
// vira o DoacaoId e retries com a mesma chave nao duplicam o evento
// publicado. Sem chave, mantem o comportamento antigo (Guid novo sempre).
public record RegistrarDoacaoCommand(Guid DoadorId, Guid CampanhaId, decimal ValorDoacao, Guid? IdempotencyKey) : IRequest<RegistrarDoacaoResult>;

public record RegistrarDoacaoResult(Guid DoacaoId);

public class RegistrarDoacaoCommandValidator : AbstractValidator<RegistrarDoacaoCommand>
{
    public RegistrarDoacaoCommandValidator()
    {
        RuleFor(x => x.CampanhaId).NotEmpty();
        RuleFor(x => x.ValorDoacao).GreaterThan(0);
    }
}

public class RegistrarDoacaoHandler(
    ICampanhaRepository campanhaRepository,
    IDoacaoProcessadaRepository doacaoProcessadaRepository,
    IDoacaoEventPublisher eventPublisher,
    TimeProvider timeProvider) : IRequestHandler<RegistrarDoacaoCommand, RegistrarDoacaoResult>
{
    public async Task<RegistrarDoacaoResult> Handle(RegistrarDoacaoCommand request, CancellationToken cancellationToken)
    {
        var campanha = await campanhaRepository.ObterPorIdAsync(request.CampanhaId, cancellationToken)
            ?? throw new DomainException("Campanha nao encontrada.");

        if (!campanha.PodeReceberDoacao())
            throw new DomainException("Nao e possivel doar para uma campanha encerrada ou cancelada.");

        var doacaoId = request.IdempotencyKey ?? Guid.NewGuid();
        var agora = timeProvider.GetUtcNow().UtcDateTime;

        if (request.IdempotencyKey is not null)
        {
            var eNova = await doacaoProcessadaRepository.TentarRegistrarAsync(
                doacaoId, campanha.Id, request.DoadorId, request.ValorDoacao, agora, cancellationToken);

            if (!eNova)
                return new RegistrarDoacaoResult(doacaoId); // retry com a mesma chave - nao republica
        }

        // Nao atualiza o valor arrecadado aqui - so publica o evento.
        // Quem soma no total da campanha e o conexao-solidaria-donation-worker,
        // ao consumir a fila (requisito explicito do desafio).
        await eventPublisher.PublicarAsync(
            new DoacaoRecebidaEvent(doacaoId, campanha.Id, request.DoadorId, request.ValorDoacao, agora),
            cancellationToken);

        return new RegistrarDoacaoResult(doacaoId);
    }
}
