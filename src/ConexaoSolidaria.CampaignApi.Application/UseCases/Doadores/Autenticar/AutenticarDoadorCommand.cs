using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Doadores.Autenticar;

public record AutenticarDoadorCommand(string Email, string Senha) : IRequest<AutenticarDoadorResult>;

public record AutenticarDoadorResult(string Token);

public class AutenticarDoadorHandler(
    IDoadorRepository repository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<AutenticarDoadorCommand, AutenticarDoadorResult>
{
    public async Task<AutenticarDoadorResult> Handle(AutenticarDoadorCommand request, CancellationToken cancellationToken)
    {
        var doador = await repository.ObterPorEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (doador is null || !passwordHasher.Verificar(request.Senha, doador.SenhaHash))
            throw new DomainException("Email ou senha invalidos.");

        var token = jwtTokenGenerator.GerarToken(doador.Id, doador.Email, Roles.Doador);

        return new AutenticarDoadorResult(token);
    }
}
