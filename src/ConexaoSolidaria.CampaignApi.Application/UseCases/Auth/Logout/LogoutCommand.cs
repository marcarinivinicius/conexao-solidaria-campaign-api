using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

// Idempotente de proposito: token desconhecido ou ja revogado nao e erro,
// so um no-op - nao vaza pra quem chama se o token era valido ou nao.
public class LogoutHandler(
    IRefreshTokenRepository repository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);
        var existente = await repository.ObterPorTokenHashAsync(tokenHash, cancellationToken);

        if (existente is null || existente.RevogadoEm is not null)
            return;

        existente.Revogar(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);
    }
}
