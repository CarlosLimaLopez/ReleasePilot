using MediatR;
using ReleasePilot.Domain.Enums;

namespace ReleasePilot.Application.Queries.GetEnvironmentStatus
{
    public record EnvironmentStatusDto(
        DeploymentEnvironment Environment,
        Guid CurrentPromotionId,
        string Version,
        PromotionState State);

    public record GetEnvironmentStatusQuery(string ApplicationId) : IRequest<IEnumerable<EnvironmentStatusDto>>;
}