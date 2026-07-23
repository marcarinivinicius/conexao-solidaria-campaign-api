using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Xunit;

namespace ConexaoSolidaria.CampaignApi.Domain.Tests;

public class RefreshTokenTests
{
    private static readonly DateTime Agora = new(2026, 1, 1);

    [Fact]
    public void Criar_ComDadosValidos_DeveDefinirExpiracaoCorreta()
    {
        var token = RefreshToken.Criar(Guid.NewGuid(), "Doador", "joao@exemplo.com", "hash", Agora, TimeSpan.FromDays(7));

        Assert.Equal(Agora.AddDays(7), token.ExpiraEm);
        Assert.Null(token.RevogadoEm);
        Assert.Null(token.SubstituidoPorId);
    }

    [Fact]
    public void EstaValido_AntesDeExpirarESemRevogar_DeveSerTrue()
    {
        var token = RefreshToken.Criar(Guid.NewGuid(), "Doador", "joao@exemplo.com", "hash", Agora, TimeSpan.FromDays(7));

        Assert.True(token.EstaValido(Agora.AddDays(1)));
    }

    [Fact]
    public void EstaValido_DepoisDeExpirar_DeveSerFalse()
    {
        var token = RefreshToken.Criar(Guid.NewGuid(), "Doador", "joao@exemplo.com", "hash", Agora, TimeSpan.FromDays(7));

        Assert.False(token.EstaValido(Agora.AddDays(8)));
    }

    [Fact]
    public void Revogar_DeveMarcarRevogadoEmESubstituidoPorId()
    {
        var token = RefreshToken.Criar(Guid.NewGuid(), "Doador", "joao@exemplo.com", "hash", Agora, TimeSpan.FromDays(7));
        var substitutoId = Guid.NewGuid();

        token.Revogar(Agora.AddHours(1), substitutoId);

        Assert.Equal(Agora.AddHours(1), token.RevogadoEm);
        Assert.Equal(substitutoId, token.SubstituidoPorId);
        Assert.False(token.EstaValido(Agora.AddHours(2)));
    }
}
