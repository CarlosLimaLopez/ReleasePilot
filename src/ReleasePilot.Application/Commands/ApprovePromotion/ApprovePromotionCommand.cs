using System;
using MediatR;

namespace ReleasePilot.Application.Commands.ApprovePromotion
{
    public record ApprovePromotionCommand(Guid PromotionId, string ActingUser) : IRequest;
}