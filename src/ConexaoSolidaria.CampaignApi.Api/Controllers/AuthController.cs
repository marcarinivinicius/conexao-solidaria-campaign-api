using ConexaoSolidaria.CampaignApi.Application;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Login;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Doadores.CadastrarDoador;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Gestores.CadastrarGestor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

// Login unico pra qualquer role (SuperAdmin, GestorONG, Doador) - o
// backend resolve sozinho quem e quem, o cliente so manda email/senha.
[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Senha), cancellationToken);
        return Ok(result);
    }

    [HttpPost("register/doador")]
    [AllowAnonymous]
    public async Task<ActionResult<CadastrarDoadorResult>> RegistrarDoador(
        [FromBody] CadastrarDoadorRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CadastrarDoadorCommand(request.NomeCompleto, request.Email, request.Cpf, request.Senha),
            cancellationToken);

        return CreatedAtAction(nameof(RegistrarDoador), new { }, result);
    }

    [HttpPost("register/gestor")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<ActionResult<CadastrarGestorResult>> RegistrarGestor(
        [FromBody] CadastrarGestorRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CadastrarGestorCommand(request.NomeCompleto, request.Email, request.Senha),
            cancellationToken);

        return CreatedAtAction(nameof(RegistrarGestor), new { }, result);
    }
}

public record LoginRequest(string Email, string Senha);
public record CadastrarDoadorRequest(string NomeCompleto, string Email, string Cpf, string Senha);
public record CadastrarGestorRequest(string NomeCompleto, string Email, string Senha);
