using MediatR;
using ReleasePilot.Domain.Enums;

namespace ReleasePilot.Application.Queries.GetPromotionById
{
    public record WorkItemDetailsDto(string Id, string Title, string Status);

    public record PromotionDetailsDto(
        Guid Id,
        string ApplicationId,
        string Version,
        DeploymentEnvironment SourceEnvironment,
        DeploymentEnvironment TargetEnvironment,
        PromotionState State,
        string? RollbackReason,
        IEnumerable<WorkItemDetailsDto> WorkItems);

    public record GetPromotionByIdQuery(Guid PromotionId) : IRequest<PromotionDetailsDto?>;
}