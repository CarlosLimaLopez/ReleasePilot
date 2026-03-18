using MediatR;
using ReleasePilot.Domain.Repositories;
using ReleasePilot.Domain.Services;

namespace ReleasePilot.Application.Commands.ApprovePromotion
{
    public class ApprovePromotionCommandHandler(
        IPromotionRepository repository,
        IPromotionAuthorizationPolicy authPolicy,
        IPromotionConcurrencyPolicy concurrencyPolicy) 
        : IRequestHandler<ApprovePromotionCommand>
    {
        public async Task Handle(ApprovePromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await repository.GetByIdAsync(request.PromotionId, cancellationToken)
                ?? throw new ApplicationException($"Promotion {request.PromotionId} not found.");

            promotion.Approve(request.ActingUser, authPolicy, concurrencyPolicy);

            await repository.UpdateAsync(promotion, cancellationToken);
        }
    }
}