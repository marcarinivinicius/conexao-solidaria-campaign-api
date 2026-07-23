using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using ConexaoSolidaria.CampaignApi.Domain.Validation;

namespace ConexaoSolidaria.CampaignApi.Domain.Entities;

// Gestor da ONG (role GestorONG). Diferente do Doador, so e criado por
// quem ja autentica como SuperAdmin - nao ha auto-cadastro publico.
public class Gestor
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    private Gestor() { }

    public static Gestor Criar(string nomeCompleto, string email, string senhaHash, DateTime agora)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new DomainException("Nome completo e obrigatorio.");

        if (!EmailValidator.EhValido(email))
            throw new DomainException("Email invalido.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("Senha e obrigatoria.");

        return new Gestor
        {
            Id = Guid.NewGuid(),
            NomeCompleto = nomeCompleto,
            Email = email.Trim().ToLowerInvariant(),
            SenhaHash = senhaHash,
            CriadoEm = agora
        };
    }
}
