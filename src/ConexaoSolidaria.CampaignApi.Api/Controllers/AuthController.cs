using ConexaoSolidaria.CampaignApi.Application;
using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

// Login do GestorONG: credencial unica seedada via configuracao (secao
// "GestorOng"), sem tela de auto-cadastro - fora do escopo do desafio, que
// so pede a role existindo e os endpoints de gestao bloqueados pra ela.
[ApiController]
[Route("api/v1/auth/gestor")]
[AllowAnonymous]
public class AuthController(
    IOptions<GestorOngCredentialsOptions> credentials,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginGestorResponse> Login([FromBody] LoginGestorRequest request)
    {
        var config = credentials.Value;

        if (!string.Equals(request.Email, config.Email, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(config.SenhaHash)
            || !passwordHasher.Verificar(request.Senha, config.SenhaHash))
        {
            return Unauthorized(new { message = "Email ou senha invalidos." });
        }

        var token = jwtTokenGenerator.GerarToken(Guid.Empty, config.Email, Roles.GestorOng);

        return Ok(new LoginGestorResponse(token));
    }
}

public record LoginGestorRequest(string Email, string Senha);

public record LoginGestorResponse(string Token);
