namespace ConexaoSolidaria.CampaignApi.Domain.Exceptions;

// Um refresh token ja revogado foi apresentado de novo - sinal de
// roubo/replay (rotacao invalida reuso por design). Quem trata isso
// revoga toda a sessao do usuario como resposta defensiva.
public class RefreshTokenReusedException(string message) : Exception(message);
