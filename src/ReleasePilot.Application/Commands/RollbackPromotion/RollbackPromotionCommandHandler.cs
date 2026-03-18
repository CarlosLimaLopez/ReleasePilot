using MediatR;
using ReleasePilot.Domain.Repositories;

namespace ReleasePilot.Application.Commands.RollbackPromotion
{
    public class RollbackPromotionCommandHandler(
        IPromotionRepository repository) 
        : IRequestHandler<RollbackPromotionCommand>
    {
        public async Task Handle(RollbackPromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await repository.GetByIdAsync(request.PromotionId, cancellationToken)
                ?? throw new ApplicationException($"Promotion {request.PromotionId} not found.");

            promotion.Rollback(request.Reason);

            await repository.UpdateAsync(promotion, cancellationToken);
        }
    }
}