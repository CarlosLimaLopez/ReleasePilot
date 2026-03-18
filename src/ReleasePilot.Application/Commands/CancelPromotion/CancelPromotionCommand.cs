using MediatR;

namespace ReleasePilot.Application.Commands.CancelPromotion
{
    public record CancelPromotionCommand(Guid PromotionId) : IRequest;
}