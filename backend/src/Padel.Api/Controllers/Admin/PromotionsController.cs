using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Padel.Application.Promotions;

namespace Padel.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/promotions")]
public sealed class PromotionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PromotionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var promotions = await sender.Send(new GetPromotionsQuery(), cancellationToken);
        return Ok(promotions);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreatePromotionCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Created($"api/admin/promotions/{id}", null);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<PromotionDto>> Update(
        long id,
        UpdatePromotionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePromotionCommand(
            id, request.Name, request.IsActive, request.StartDate, request.EndDate, request.Rules);
        var promotion = await sender.Send(command, cancellationToken);
        return Ok(promotion);
    }
}

public sealed record UpdatePromotionRequest(
    string Name,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<PricingRuleInput> Rules);
