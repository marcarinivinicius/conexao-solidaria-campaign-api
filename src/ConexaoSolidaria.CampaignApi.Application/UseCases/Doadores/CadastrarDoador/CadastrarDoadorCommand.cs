using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Doadores.CadastrarDoador;

public record CadastrarDoadorCommand(
    string NomeCompleto,
    string Email,
    string Cpf,
    string Senha) : IRequest<CadastrarDoadorResult>;

// Valida so a forma (nao-vazio, formato, tamanho) - o checksum de CPF e a
// unicidade de email continuam na factory de dominio/handler, nao aqui.
public class CadastrarDoadorCommandValidator : AbstractValidator<CadastrarDoadorCommand>
{
    public CadastrarDoadorCommandValidator()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Cpf).NotEmpty().Length(11);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8);
    }
}

public record CadastrarDoadorResult(Guid Id, string Token, string RefreshToken, string Role);

public class CadastrarDoadorHandler(
    IDoadorRepository repository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ITokenIssuer tokenIssuer,
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

        var tokens = await tokenIssuer.EmitirAsync(doador.Id, doador.Email, Roles.Doador, cancellationToken);

        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CadastrarDoadorResult(doador.Id, tokens.AccessToken, tokens.RefreshToken, tokens.Role);
    }
}
