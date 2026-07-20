using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Enums;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using Xunit;

namespace ConexaoSolidaria.CampaignApi.Domain.Tests;

public class CampanhaTests
{
    private static readonly DateTime Agora = new(2026, 1, 1);

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarCampanhaAtiva()
    {
        var campanha = Campanha.Criar(
            "Natal Solidario",
            "Arrecadacao de brinquedos",
            Agora,
            Agora.AddMonths(1),
            1000m,
            Agora);

        Assert.Equal(StatusCampanha.Ativa, campanha.Status);
        Assert.Equal(0, campanha.ValorTotalArrecadado);
        Assert.True(campanha.PodeReceberDoacao());
    }

    [Fact]
    public void Criar_ComDataFimNoPassado_DeveLancarExcecao()
    {
        var ex = Assert.Throws<DomainException>(() => Campanha.Criar(
            "Titulo",
            "Descricao",
            Agora.AddDays(-10),
            Agora.AddDays(-1),
            1000m,
            Agora));

        Assert.Contains("passado", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Criar_ComMetaFinanceiraMenorOuIgualAZero_DeveLancarExcecao(decimal meta)
    {
        var ex = Assert.Throws<DomainException>(() => Campanha.Criar(
            "Titulo",
            "Descricao",
            Agora,
            Agora.AddMonths(1),
            meta,
            Agora));

        Assert.Contains("Meta financeira", ex.Message);
    }

    [Fact]
    public void PodeReceberDoacao_QuandoCancelada_DeveRetornarFalso()
    {
        var campanha = Campanha.Criar("Titulo", "Descricao", Agora, Agora.AddMonths(1), 1000m, Agora);
        campanha.Editar("Titulo", "Descricao", Agora, Agora.AddMonths(1), 1000m, StatusCampanha.Cancelada, Agora);

        Assert.False(campanha.PodeReceberDoacao());
    }
}
