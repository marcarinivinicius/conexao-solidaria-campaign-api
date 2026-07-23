using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace ConexaoSolidaria.CampaignApi.Application.UseCases.Gestores.CadastrarGestor;

// So chamado por quem ja autenticou como SuperAdmin - o [Authorize] fica
// no controller, aqui so a regra de negocio (email unico).
public record CadastrarGestorCommand(string NomeCompleto, string Email, string Senha) : IRequest<CadastrarGestorResult>;

public class CadastrarGestorCommandValidator : AbstractValidator<CadastrarGestorCommand>
{
    public CadastrarGestorCommandValidator()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8);
    }
}

public record CadastrarGestorResult(Guid Id);

public class CadastrarGestorHandler(
    IGestorRepository repository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<CadastrarGestorCommand, CadastrarGestorResult>
{
    public async Task<CadastrarGestorResult> Handle(CadastrarGestorCommand request, CancellationToken cancellationToken)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        if (await repository.ExisteComEmailAsync(emailNormalizado, cancellationToken))
            throw new DomainException("Ja existe um gestor cadastrado com esse email.");

        var senhaHash = passwordHasher.Hash(request.Senha);
        var agora = timeProvider.GetUtcNow().UtcDateTime;

        var gestor = Gestor.Criar(request.NomeCompleto, emailNormalizado, senhaHash, agora);

        await repository.AdicionarAsync(gestor, cancellationToken);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new CadastrarGestorResult(gestor.Id);
    }
}
