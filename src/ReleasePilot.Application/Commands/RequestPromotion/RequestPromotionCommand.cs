using MediatR;
using ReleasePilot.Domain.Enums;

namespace ReleasePilot.Application.Commands.RequestPromotion
{
    public record WorkItemDto(string IssueId);

    public record RequestPromotionCommand(
        string ApplicationId, 
        string Version, 
        DeploymentEnvironment TargetEnvironment,
        DeploymentEnvironment SourceEnvironment,
        string ActingUser,
        IEnumerable<WorkItemDto>? WorkItems) : IRequest<Guid>;
}