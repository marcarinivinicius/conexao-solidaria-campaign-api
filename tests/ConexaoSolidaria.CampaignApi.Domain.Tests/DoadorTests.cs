using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using Xunit;

namespace ConexaoSolidaria.CampaignApi.Domain.Tests;

public class DoadorTests
{
    private static readonly DateTime Agora = new(2026, 1, 1);

    [Fact]
    public void Criar_ComCpfValido_DeveCriarDoador()
    {
        var doador = Doador.Criar("Maria Silva", "maria@exemplo.com", "529.982.247-25", "hash", Agora);

        Assert.Equal("52998224725", doador.Cpf);
        Assert.Equal("maria@exemplo.com", doador.Email);
    }

    [Theory]
    [InlineData("111.111.111-11")] // todos digitos iguais
    [InlineData("123.456.789-00")] // digitos verificadores errados
    [InlineData("123")]            // tamanho invalido
    public void Criar_ComCpfInvalido_DeveLancarExcecao(string cpf)
    {
        Assert.Throws<DomainException>(() => Doador.Criar("Maria Silva", "maria@exemplo.com", cpf, "hash", Agora));
    }

    [Fact]
    public void Criar_ComEmailInvalido_DeveLancarExcecao()
    {
        Assert.Throws<DomainException>(() => Doador.Criar("Maria Silva", "nao-e-email", "529.982.247-25", "hash", Agora));
    }
}
