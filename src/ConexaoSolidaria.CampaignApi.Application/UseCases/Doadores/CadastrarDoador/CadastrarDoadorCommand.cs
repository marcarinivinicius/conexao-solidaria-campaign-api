using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Doadores.CadastrarDoador;

public record CadastrarDoadorCommand(
    string NomeCompleto,
    string Email,
    string Cpf,
    string Senha) : IRequest<CadastrarDoadorResult>;

public record CadastrarDoadorResult(Guid Id);

public class CadastrarDoadorHandler(
    IDoadorRepository repository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CadastrarDoadorCommand, CadastrarDoadorResult>
{
    public async Task<CadastrarDoadorResult> Handle(CadastrarDoadorCommand request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        if (await repository.ExisteComEmailAsync(emailNormalizado, cancellationToken))
            throw new DomainException("Ja existe um doador cadastrado com esse email.");

        var senhaHash = passwordHasher.Hash(request.Senha);
        var agora = timeProvider.GetUtcNow().UtcDateTime;

        var doador = Doador.Criar(request.NomeCompleto, emailNormalizado, request.Cpf, senhaHash, agora);

        await repository.AdicionarAsync(doador, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CadastrarDoadorResult(doador.Id);
    }
}
