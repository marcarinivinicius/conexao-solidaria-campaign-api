using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using Xunit;

namespace ConexaoSolidaria.CampaignApi.Domain.Tests;

public class GestorTests
{
    private static readonly DateTime Agora = new(2026, 1, 1);

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarGestor()
    {
        var gestor = Gestor.Criar("Joao Souza", "Joao@Exemplo.com", "hash", Agora);

        Assert.Equal("joao@exemplo.com", gestor.Email);
        Assert.Equal("Joao Souza", gestor.NomeCompleto);
    }

    [Fact]
    public void Criar_ComEmailInvalido_DeveLancarExcecao()
    {
        Assert.Throws<DomainException>(() => Gestor.Criar("Joao Souza", "nao-e-email", "hash", Agora));
    }

    [Fact]
    public void Criar_ComNomeVazio_DeveLancarExcecao()
    {
        Assert.Throws<DomainException>(() => Gestor.Criar("", "joao@exemplo.com", "hash", Agora));
    }

    [Fact]
    public void Criar_ComSenhaHashVazia_DeveLancarExcecao()
    {
        Assert.Throws<DomainException>(() => Gestor.Criar("Joao Souza", "joao@exemplo.com", "", Agora));
    }
}
