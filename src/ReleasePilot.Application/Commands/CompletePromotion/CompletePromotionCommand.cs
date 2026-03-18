using System;
using MediatR;

namespace ReleasePilot.Application.Commands.CompletePromotion
{
    public record CompletePromotionCommand(Guid PromotionId, string ActingUser) : IRequest;
}