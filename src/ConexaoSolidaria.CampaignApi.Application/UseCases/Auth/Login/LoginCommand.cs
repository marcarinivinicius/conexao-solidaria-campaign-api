using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Login;

// Login unico, independente da role - resolve na ordem SuperAdmin ->
// Gestor -> Doador e devolve um token com a role certa.
public record LoginCommand(string Email, string Senha) : IRequest<LoginResult>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty();
    }
}

public record LoginResult(string Token, string RefreshToken, string Role);

public class LoginHandler(
    ISuperAdminAuthenticator superAdminAuthenticator,
    IGestorRepository gestorRepository,
    IDoadorRepository doadorRepository,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (superAdminAuthenticator.Autenticar(email, request.Senha))
            return await EmitirAsync(Guid.Empty, email, Roles.SuperAdmin, cancellationToken);

        var gestor = await gestorRepository.ObterPorEmailAsync(email, cancellationToken);
        if (gestor is not null && passwordHasher.Verificar(request.Senha, gestor.SenhaHash))
            return await EmitirAsync(gestor.Id, gestor.Email, Roles.GestorOng, cancellationToken);

        var doador = await doadorRepository.ObterPorEmailAsync(email, cancellationToken);
        if (doador is not null && passwordHasher.Verificar(request.Senha, doador.SenhaHash))
            return await EmitirAsync(doador.Id, doador.Email, Roles.Doador, cancellationToken);

        throw new DomainException("Email ou senha invalidos.");
    }

    private async Task<LoginResult> EmitirAsync(Guid usuarioId, string email, string role, CancellationToken cancellationToken)
    {
        var tokens = await tokenIssuer.EmitirAsync(usuarioId, email, role, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);
        return new LoginResult(tokens.AccessToken, tokens.RefreshToken, tokens.Role);
    }
}
