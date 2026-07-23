using ConexaoSolidaria.CampaignApi.Application;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Gestores.CadastrarGestor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

[ApiController]
[Route("api/v1/gestores")]
[Authorize(Roles = Roles.SuperAdmin)]
public class GestoresController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CadastrarGestorResult>> Cadastrar(
        [FromBody] CadastrarGestorRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CadastrarGestorCommand(request.NomeCompleto, request.Email, request.Senha),
            cancellationToken);

        return CreatedAtAction(nameof(Cadastrar), new { }, result);
    }
}

public record CadastrarGestorRequest(string NomeCompleto, string Email, string Senha);
