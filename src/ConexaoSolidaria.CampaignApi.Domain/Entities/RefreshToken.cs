namespace ConexaoSolidaria.CampaignApi.Domain.Entities;

// Sessao de refresh token com rotacao: cada uso gera um novo (SubstituidoPorId
// encadeia a rotacao) e revoga o antigo. Se um token ja revogado for
// apresentado de novo, e sinal de roubo/replay (ver RefreshTokenReusedException).
// UsuarioId+Role identifica o dono (inclusive SuperAdmin, que nao tem linha em
// nenhuma tabela de usuario - mesmo esquema que ja usa nas claims do JWT).
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; }
    public DateTime ExpiraEm { get; private set; }
    public DateTime? RevogadoEm { get; private set; }
    public Guid? SubstituidoPorId { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Criar(Guid usuarioId, string role, string email, string tokenHash, DateTime agora, TimeSpan tempoDeVida)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Role = role,
            Email = email,
            TokenHash = tokenHash,
            CriadoEm = agora,
            ExpiraEm = agora.Add(tempoDeVida)
        };
    }

    public bool EstaValido(DateTime agora) => RevogadoEm is null && agora < ExpiraEm;

    public void Revogar(DateTime agora, Guid? substituidoPorId = null)
    {
        RevogadoEm = agora;
        SubstituidoPorId = substituidoPorId;
    }
}
