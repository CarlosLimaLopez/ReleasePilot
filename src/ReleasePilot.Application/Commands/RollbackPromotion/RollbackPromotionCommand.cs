using System;
using MediatR;

namespace ReleasePilot.Application.Commands.RollbackPromotion
{
    public record RollbackPromotionCommand(Guid PromotionId, string Reason) : IRequest;
}