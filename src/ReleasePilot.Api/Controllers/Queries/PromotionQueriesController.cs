using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReleasePilot.Application.Queries.GetEnvironmentStatus;
using ReleasePilot.Application.Queries.GetPromotionById;
using ReleasePilot.Application.Queries.ListPromotionsByApplication;

namespace ReleasePilot.Api.Controllers.Queries;

[ApiController]
[Route("api")]
public class PromotionQueriesController(ISender sender) : ControllerBase
{
    [HttpGet("promotions/{id:guid}")]
    [ActionName("GetPromotionById")]
    public async Task<IActionResult> GetPromotionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPromotionByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("applications/{applicationId}/environment-status")]
    public async Task<IActionResult> GetEnvironmentStatus(
        string applicationId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEnvironmentStatusQuery(applicationId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("applications/{applicationId}/promotions")]
    public async Task<IActionResult> ListPromotionsByApplication(
        string applicationId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new ListPromotionsByApplicationQuery(applicationId, pageIndex, pageSize),
            cancellationToken);
        return Ok(result);
    }
}