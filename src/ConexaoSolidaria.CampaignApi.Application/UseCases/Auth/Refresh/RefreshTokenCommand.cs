using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public record RefreshTokenResult(string AccessToken, string RefreshToken, string Role);

public class RefreshTokenHandler(
    IRefreshTokenRepository repository,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);

        var existente = await repository.ObterPorTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new InvalidRefreshTokenException("Refresh token invalido.");

        if (existente.RevogadoEm is not null)
        {
            // Token ja usado apresentado de novo - possivel roubo/replay.
            // Revoga toda a sessao ativa do usuario como resposta defensiva.
            await repository.RevogarAtivosAsync(existente.UsuarioId, existente.Role, agora, cancellationToken);
            await unitOfWork.SalvarAlteracoesAsync(cancellationToken);
            throw new RefreshTokenReusedException("Refresh token ja utilizado. Sessao revogada por seguranca.");
        }

        if (!existente.EstaValido(agora))
            throw new RefreshTokenExpiredException("Refresh token expirado.");

        var tokens = await tokenIssuer.EmitirAsync(existente.UsuarioId, existente.Email, existente.Role, cancellationToken);
        existente.Revogar(agora, tokens.RefreshTokenId);

        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new RefreshTokenResult(tokens.AccessToken, tokens.RefreshToken, tokens.Role);
    }
}
