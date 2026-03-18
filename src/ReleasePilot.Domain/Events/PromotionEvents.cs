using System;

namespace ReleasePilot.Domain.Events
{
    public record PromotionRequested(Guid PromotionId) : IDomainEvent;
    public record PromotionApproved(Guid PromotionId) : IDomainEvent;
    public record DeploymentStarted(Guid PromotionId) : IDomainEvent;
    public record PromotionCompleted(Guid PromotionId) : IDomainEvent;
    public record PromotionRolledBack(Guid PromotionId, string Reason) : IDomainEvent;
    public record PromotionCancelled(Guid PromotionId) : IDomainEvent;
}