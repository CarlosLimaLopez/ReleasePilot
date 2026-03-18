using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReleasePilot.Application.Commands.ApprovePromotion;
using ReleasePilot.Application.Commands.CancelPromotion;
using ReleasePilot.Application.Commands.CompletePromotion;
using ReleasePilot.Application.Commands.RequestPromotion;
using ReleasePilot.Application.Commands.RollbackPromotion;
using ReleasePilot.Application.Commands.StartDeployment;

namespace ReleasePilot.Api.Controllers.Commands;

[ApiController]
[Route("api/promotions")]
public class PromotionCommandsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RequestPromotion(
        [FromBody] RequestPromotionCommand command,
        CancellationToken cancellationToken)
    {
        var promotionId = await sender.Send(command, cancellationToken);

        return Created($"/api/promotions/{promotionId}", new { id = promotionId });
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> ApprovePromotion(
        Guid id,
        [FromBody] ApprovePromotionCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { PromotionId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/start-deployment")]
    public async Task<IActionResult> StartDeployment(
        Guid id,
        [FromBody] StartDeploymentCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { PromotionId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> CompletePromotion(
        Guid id,
        [FromBody] CompletePromotionCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { PromotionId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/rollback")]
    public async Task<IActionResult> RollbackPromotion(
        Guid id,
        [FromBody] RollbackPromotionCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { PromotionId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> CancelPromotion(
        Guid id,
        [FromBody] CancelPromotionCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command with { PromotionId = id }, cancellationToken);
        return NoContent();
    }
}