using ConexaoSolidaria.CampaignApi.Application.UseCases.Doadores.CadastrarDoador;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

[ApiController]
[Route("api/v1/doadores")]
[AllowAnonymous]
public class DoadoresController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CadastrarDoadorResult>> Cadastrar(
        [FromBody] CadastrarDoadorRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CadastrarDoadorCommand(request.NomeCompleto, request.Email, request.Cpf, request.Senha),
            cancellationToken);

        return CreatedAtAction(nameof(Cadastrar), new { }, result);
    }
}

public record CadastrarDoadorRequest(string NomeCompleto, string Email, string Cpf, string Senha);
