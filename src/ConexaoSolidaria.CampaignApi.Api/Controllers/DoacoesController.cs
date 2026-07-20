using System.IdentityModel.Tokens.Jwt;
using ConexaoSolidaria.CampaignApi.Application;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Doacoes.RegistrarDoacao;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

[ApiController]
[Route("api/v1/doacoes")]
[Authorize(Roles = Roles.Doador)]
public class DoacoesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RegistrarDoacaoResult>> Registrar(
        [FromBody] RegistrarDoacaoRequest request, CancellationToken cancellationToken)
    {
        var doadorIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new InvalidOperationException("Token sem claim sub.");

        var result = await mediator.Send(
            new RegistrarDoacaoCommand(Guid.Parse(doadorIdClaim), request.IdCampanha, request.ValorDoacao),
            cancellationToken);

        return Accepted(result);
    }
}

public record RegistrarDoacaoRequest(Guid IdCampanha, decimal ValorDoacao);
