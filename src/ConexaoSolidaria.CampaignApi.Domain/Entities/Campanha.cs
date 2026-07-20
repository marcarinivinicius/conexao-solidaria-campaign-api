using ConexaoSolidaria.CampaignApi.Domain.Enums;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;

namespace ConexaoSolidaria.CampaignApi.Domain.Entities;

public class Campanha
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public decimal MetaFinanceira { get; private set; }
    public StatusCampanha Status { get; private set; }
    public decimal ValorTotalArrecadado { get; private set; }
    public DateTime CriadaEm { get; private set; }

    private Campanha() { }

    public static Campanha Criar(
        string titulo,
        string descricao,
        DateTime dataInicio,
        DateTime dataFim,
        decimal metaFinanceira,
        DateTime agora)
    {
        ValidarDados(titulo, descricao, dataInicio, dataFim, metaFinanceira, agora);

        return new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = titulo,
            Descricao = descricao,
            DataInicio = dataInicio,
            DataFim = dataFim,
            MetaFinanceira = metaFinanceira,
            Status = StatusCampanha.Ativa,
            ValorTotalArrecadado = 0,
            CriadaEm = agora
        };
    }

    public void Editar(
        string titulo,
        string descricao,
        DateTime dataInicio,
        DateTime dataFim,
        decimal metaFinanceira,
        StatusCampanha status,
        DateTime agora)
    {
        ValidarDados(titulo, descricao, dataInicio, dataFim, metaFinanceira, agora);

        Titulo = titulo;
        Descricao = descricao;
        DataInicio = dataInicio;
        DataFim = dataFim;
        MetaFinanceira = metaFinanceira;
        Status = status;
    }

    public bool PodeReceberDoacao() => Status == StatusCampanha.Ativa;

    private static void ValidarDados(
        string titulo,
        string descricao,
        DateTime dataInicio,
        DateTime dataFim,
        decimal metaFinanceira,
        DateTime agora)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("Titulo e obrigatorio.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descricao e obrigatoria.");

        if (dataFim < agora)
            throw new DomainException("Data de termino nao pode estar no passado.");

        if (dataFim <= dataInicio)
            throw new DomainException("Data de termino deve ser posterior a data de inicio.");

        if (metaFinanceira <= 0)
            throw new DomainException("Meta financeira deve ser maior que zero.");
    }
}
