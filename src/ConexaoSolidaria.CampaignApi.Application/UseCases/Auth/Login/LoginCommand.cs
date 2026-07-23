using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Login;

// Login unico, independente da role - resolve na ordem SuperAdmin ->
// Gestor -> Doador e devolve um token com a role certa.
public record LoginCommand(string Email, string Senha) : IRequest<LoginResult>;

public record LoginResult(string Token, string Role);

public class LoginHandler(
    ISuperAdminAuthenticator superAdminAuthenticator,
    IGestorRepository gestorRepository,
    IDoadorRepository doadorRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (superAdminAuthenticator.Autenticar(email, request.Senha))
        {
            var token = jwtTokenGenerator.GerarToken(Guid.Empty, email, Roles.SuperAdmin);
            return new LoginResult(token, Roles.SuperAdmin);
        }

        var gestor = await gestorRepository.ObterPorEmailAsync(email, cancellationToken);
        if (gestor is not null && passwordHasher.Verificar(request.Senha, gestor.SenhaHash))
        {
            var token = jwtTokenGenerator.GerarToken(gestor.Id, gestor.Email, Roles.GestorOng);
            return new LoginResult(token, Roles.GestorOng);
        }

        var doador = await doadorRepository.ObterPorEmailAsync(email, cancellationToken);
        if (doador is not null && passwordHasher.Verificar(request.Senha, doador.SenhaHash))
        {
            var token = jwtTokenGenerator.GerarToken(doador.Id, doador.Email, Roles.Doador);
            return new LoginResult(token, Roles.Doador);
        }

        throw new DomainException("Email ou senha invalidos.");
    }
}
