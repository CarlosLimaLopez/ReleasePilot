using MediatR;
using ReleasePilot.Domain.Repositories;

namespace ReleasePilot.Application.Commands.CancelPromotion
{
    public class CancelPromotionCommandHandler(
        IPromotionRepository repository) 
        : IRequestHandler<CancelPromotionCommand>
    {
        public async Task Handle(CancelPromotionCommand request, CancellationToken cancellationToken)
        {
            var promotion = await repository.GetByIdAsync(request.PromotionId, cancellationToken)
                ?? throw new ApplicationException($"Promotion {request.PromotionId} not found.");

            promotion.Cancel();

            await repository.UpdateAsync(promotion, cancellationToken);
        }
    }
}