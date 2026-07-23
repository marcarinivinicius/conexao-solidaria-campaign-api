using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using ConexaoSolidaria.CampaignApi.Domain.Validation;

namespace ConexaoSolidaria.CampaignApi.Domain.Entities;

public class Doador
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }

    private Doador() { }

    public static Doador Criar(string nomeCompleto, string email, string cpf, string senhaHash, DateTime agora)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new DomainException("Nome completo e obrigatorio.");

        if (!EmailValidator.EhValido(email))
            throw new DomainException("Email invalido.");

        var cpfDigitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (!CpfEhValido(cpfDigitos))
            throw new DomainException("CPF invalido.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("Senha e obrigatoria.");

        return new Doador
        {
            Id = Guid.NewGuid(),
            NomeCompleto = nomeCompleto,
            Email = email.Trim().ToLowerInvariant(),
            Cpf = cpfDigitos,
            SenhaHash = senhaHash,
            CriadoEm = agora
        };
    }

    private static bool CpfEhValido(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        var numeros = cpf.Select(c => c - '0').ToArray();

        int CalcularDigito(int quantidade)
        {
            var soma = 0;
            var peso = quantidade + 1;
            for (var i = 0; i < quantidade; i++)
                soma += numeros[i] * peso--;

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        return CalcularDigito(9) == numeros[9] && CalcularDigito(10) == numeros[10];
    }
}
