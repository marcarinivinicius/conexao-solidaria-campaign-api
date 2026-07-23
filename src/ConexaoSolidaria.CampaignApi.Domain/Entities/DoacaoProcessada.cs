namespace ConexaoSolidaria.CampaignApi.Domain.Entities;

// Ledger de idempotencia, nao uma entidade de negocio. O donation-worker
// tambem escreve/le essa mesma tabela via SQL cru (nao possui migration
// propria, o schema e deste repo).
//
// CriadoEm != AplicadoEm de proposito: o campaign-api insere a linha (com
// AplicadoEm nulo) so pra travar a Idempotency-Key e evitar republicar o
// evento num retry - quem preenche AplicadoEm e o worker, no momento que
// de fato soma o valor na campanha. Se os dois usassem so "a linha existe"
// como sinal, o worker nunca aplicaria a soma pra doacoes que o campaign-api
// ja tinha registrado (a insercao dele bloquearia a dele proprio).
public class DoacaoProcessada
{
    public Guid Id { get; private set; }
    public Guid CampanhaId { get; private set; }
    public Guid DoadorId { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AplicadoEm { get; private set; }

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
