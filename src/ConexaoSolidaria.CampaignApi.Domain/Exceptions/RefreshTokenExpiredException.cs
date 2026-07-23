namespace ConexaoSolidaria.CampaignApi.Domain.Exceptions;

public class RefreshTokenExpiredException(string message) : Exception(message);
