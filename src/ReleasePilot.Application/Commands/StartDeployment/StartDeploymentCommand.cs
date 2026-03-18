using MediatR;

namespace ReleasePilot.Application.Commands.StartDeployment
{
    public record StartDeploymentCommand(Guid PromotionId, string ActingUser) : IRequest;
}