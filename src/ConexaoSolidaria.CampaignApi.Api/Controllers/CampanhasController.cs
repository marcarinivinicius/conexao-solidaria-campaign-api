using ConexaoSolidaria.CampaignApi.Application;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.CriarCampanha;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.EditarCampanha;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.ListarCampanhasAtivas;
using ConexaoSolidaria.CampaignApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConexaoSolidaria.CampaignApi.Api.Controllers;

[ApiController]
[Route("api/v1/campanhas")]
public class CampanhasController(IMediator mediator) : ControllerBase
{
    // Painel de transparencia - acesso publico, so campanhas Ativa.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CampanhaAtivaResult>>> ListarAtivas(CancellationToken cancellationToken)
    {
        var campanhas = await mediator.Send(new ListarCampanhasAtivasQuery(), cancellationToken);
        return Ok(campanhas);
    }

    [HttpPost]
    [Authorize(Roles = Roles.GestorOng)]
    public async Task<ActionResult<CriarCampanhaResult>> Criar(
        [FromBody] CriarCampanhaRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CriarCampanhaCommand(request.Titulo, request.Descricao, request.DataInicio, request.DataFim, request.MetaFinanceira),
            cancellationToken);

        return CreatedAtAction(nameof(ListarAtivas), new { }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.GestorOng)]
    public async Task<IActionResult> Editar(
        Guid id, [FromBody] EditarCampanhaRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new EditarCampanhaCommand(id, request.Titulo, request.Descricao, request.DataInicio, request.DataFim, request.MetaFinanceira, request.Status),
            cancellationToken);

        return NoContent();
    }
}

public record CriarCampanhaRequest(string Titulo, string Descricao, DateTime DataInicio, DateTime DataFim, decimal MetaFinanceira);

public record EditarCampanhaRequest(string Titulo, string Descricao, DateTime DataInicio, DateTime DataFim, decimal MetaFinanceira, StatusCampanha Status);
