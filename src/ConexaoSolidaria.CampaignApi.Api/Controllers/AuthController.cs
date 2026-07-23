using ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

// Login unico pra qualquer role (SuperAdmin, GestorONG, Doador) - o
// backend resolve sozinho quem e quem, o cliente so manda email/senha.
[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Senha), cancellationToken);
        return Ok(result);
    }
}

public record LoginRequest(string Email, string Senha);
