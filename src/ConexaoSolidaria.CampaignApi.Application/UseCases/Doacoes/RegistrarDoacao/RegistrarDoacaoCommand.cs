using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Application.IntegrationEvents;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Doacoes.RegistrarDoacao;

public record RegistrarDoacaoCommand(Guid DoadorId, Guid CampanhaId, decimal ValorDoacao) : IRequest<RegistrarDoacaoResult>;

public record RegistrarDoacaoResult(Guid DoacaoId);

public class RegistrarDoacaoHandler(
    ICampanhaRepository campanhaRepository,
    IDoacaoEventPublisher eventPublisher,
    TimeProvider timeProvider) : IRequestHandler<RegistrarDoacaoCommand, RegistrarDoacaoResult>
{
    public async Task<RegistrarDoacaoResult> Handle(RegistrarDoacaoCommand request, CancellationToken cancellationToken)
    {
        if (request.ValorDoacao <= 0)
            throw new DomainException("Valor da doacao deve ser maior que zero.");

        var campanha = await campanhaRepository.ObterPorIdAsync(request.CampanhaId, cancellationToken)
            ?? throw new DomainException("Campanha nao encontrada.");

        if (!campanha.PodeReceberDoacao())
            throw new DomainException("Nao e possivel doar para uma campanha encerrada ou cancelada.");

        var doacaoId = Guid.NewGuid();
        var agora = timeProvider.GetUtcNow().UtcDateTime;

        // Nao atualiza o valor arrecadado aqui - so publica o evento.
        // Quem soma no total da campanha e o conexao-solidaria-donation-worker,
        // ao consumir a fila (requisito explicito do desafio).
        await eventPublisher.PublicarAsync(
            new DoacaoRecebidaEvent(doacaoId, campanha.Id, request.DoadorId, request.ValorDoacao, agora),
            cancellationToken);

        return new RegistrarDoacaoResult(doacaoId);
    }
}
