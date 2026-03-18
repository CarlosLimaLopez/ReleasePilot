using MediatR;
using ReleasePilot.Application.Ports;
using ReleasePilot.Domain.Repositories;

namespace ReleasePilot.Application.Commands.StartDeployment
{
    public class StartDeploymentCommandHandler(
        IPromotionRepository repository,
        IDeploymentPort deploymentPort) 
        : IRequestHandler<StartDeploymentCommand>
    {
        public async Task Handle(StartDeploymentCommand request, CancellationToken cancellationToken)
        {
            var promotion = await repository.GetByIdAsync(request.PromotionId, cancellationToken)
                ?? throw new ApplicationException($"Promotion {request.PromotionId} not found.");

            promotion.StartDeployment();

            await deploymentPort.TriggerDeploymentAsync(promotion, cancellationToken);

            await repository.UpdateAsync(promotion, cancellationToken);
        }
    }
}