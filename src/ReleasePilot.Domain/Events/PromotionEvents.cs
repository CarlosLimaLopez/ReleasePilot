namespace ReleasePilot.Domain.Events
{
    public record PromotionRequested(Guid PromotionId, string ActingUser) : IDomainEvent;
    public record PromotionApproved(Guid PromotionId, string ActingUser) : IDomainEvent;
    public record DeploymentStarted(Guid PromotionId, string ActingUser) : IDomainEvent;
    public record PromotionCompleted(Guid PromotionId, string ActingUser) : IDomainEvent;
    public record PromotionRolledBack(Guid PromotionId, string Reason, string ActingUser) : IDomainEvent;
    public record PromotionCancelled(Guid PromotionId, string ActingUser) : IDomainEvent;
}