using MediatR;
using ReleasePilot.Domain.Repositories;

namespace ReleasePilot.Application.Commands.CompletePromotion
{
    public class CompletePromotionCommandHandler(
        IPromotionRepository repository) 
        : IRequestHandler<CompletePromotionCommand>
    {
        public async Task Handle(CompletePromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await repository.GetByIdAsync(request.PromotionId, cancellationToken)
                ?? throw new ApplicationException($"Promotion {request.PromotionId} not found.");

            promotion.Complete(request.ActingUser); 

            await repository.UpdateAsync(promotion, cancellationToken);
        }
    }
}