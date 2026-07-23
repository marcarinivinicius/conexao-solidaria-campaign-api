namespace ConexaoSolidaria.CampaignApi.Domain.Entities;

// Ledger de idempotencia, nao uma entidade de negocio - so registra que um
// DoacaoId ja foi processado (evita duplicar doacao em retry do cliente ou
// redelivery do RabbitMQ). O donation-worker tambem escreve/le essa mesma
// tabela via SQL cru (nao possui migration propria, o schema e deste repo).
public class DoacaoProcessada
{
    public Guid Id { get; private set; }
    public Guid CampanhaId { get; private set; }
    public Guid DoadorId { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime CriadoEm { get; private set; }

    private DoacaoProcessada() { }

    public static DoacaoProcessada Criar(Guid id, Guid campanhaId, Guid doadorId, decimal valor, DateTime agora) => new()
    {
        Id = id,
        CampanhaId = campanhaId,
        DoadorId = doadorId,
        Valor = valor,
        CriadoEm = agora
    };
}
