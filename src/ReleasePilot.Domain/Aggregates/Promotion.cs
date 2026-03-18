using ReleasePilot.Domain.Enums;
using ReleasePilot.Domain.Events;
using ReleasePilot.Domain.Exceptions;
using ReleasePilot.Domain.Services;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Domain.Aggregates
{
    public class Promotion : AggregateRoot
    {
        private static readonly DeploymentEnvironment[] EnvironmentProgression = 
        [
            DeploymentEnvironment.Dev, 
            DeploymentEnvironment.Staging, 
            DeploymentEnvironment.Production
        ];

        private readonly List<WorkItemReference> _workItemReferences = [];

        public Guid Id { get; private set; }
        public string ApplicationId { get; private set; } = string.Empty;
        public ApplicationVersion Version { get; private set; } = default!;        
        public DeploymentEnvironment SourceEnvironment { get; private set; }
        public DeploymentEnvironment TargetEnvironment { get; private set; }
        public PromotionState State { get; private set; }
        public string? RollbackReason { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? CompletedAtUtc { get; private set; }
        
        public IReadOnlyCollection<WorkItemReference> WorkItemReferences => _workItemReferences.AsReadOnly();

        private Promotion() { }

        public static Promotion Request(
            string applicationId, 
            string version, 
            DeploymentEnvironment sourceEnvironment, 
            DeploymentEnvironment targetEnvironment,
            IEnumerable<WorkItemReference>? workItems = null)
        {
            EnsureValidEnvironmentProgression(sourceEnvironment, targetEnvironment);

            var applicationVersion = ApplicationVersion.Create(version);

            var promotion = new Promotion
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Version = applicationVersion,
                SourceEnvironment = sourceEnvironment,
                TargetEnvironment = targetEnvironment,
                State = PromotionState.Requested,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (workItems != null)
                promotion._workItemReferences.AddRange(workItems);
            
            promotion.AddDomainEvent(new PromotionRequested(promotion.Id));

            return promotion;
        }

        public void Approve(
            string actingUser,
            IPromotionAuthorizationPolicy authorizationPolicy,
            IPromotionConcurrencyPolicy concurrencyPolicy)
        {
            EnsureMutable();

            if (State != PromotionState.Requested)
                throw new DomainException($"Cannot approve promotion in state: {State}");

            if (!authorizationPolicy.HasApproverRole(actingUser))
                throw new DomainException("Only users with the Approver role can approve a promotion.");

            if (concurrencyPolicy.HasExistingInProgress(ApplicationId, TargetEnvironment))
                throw new DomainException("Another promotion is already InProgress for this application and environment.");

            State = PromotionState.Approved;
            AddDomainEvent(new PromotionApproved(Id));
        }

        public void StartDeployment()
        {
            EnsureMutable();

            if (State != PromotionState.Approved)
                throw new DomainException($"Cannot start deployment from state: {State}. Must be Approved.");

            State = PromotionState.InProgress;
            AddDomainEvent(new DeploymentStarted(Id));
        }

        public void Complete()
        {
            EnsureMutable();

            if (State != PromotionState.InProgress)
                throw new DomainException($"Cannot complete promotion from state: {State}. Must be InProgress.");

            State = PromotionState.Completed;
            CompletedAtUtc = DateTimeOffset.UtcNow;
            AddDomainEvent(new PromotionCompleted(Id));
        }

        public void Rollback(string reason)
        {
            EnsureMutable();

            if (State != PromotionState.InProgress)
                throw new DomainException($"Cannot rollback promotion from state: {State}. Must be InProgress.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("A rollback reason must be provided.");

            State = PromotionState.RolledBack;
            RollbackReason = reason;
            AddDomainEvent(new PromotionRolledBack(Id, reason));
        }

        public void Cancel()
        {
            EnsureMutable();

            if (State != PromotionState.Requested)
                throw new DomainException($"Cannot cancel promotion from state: {State}. It can only be cancelled from Requested state.");

            State = PromotionState.Cancelled;
            AddDomainEvent(new PromotionCancelled(Id));
        }

        private void EnsureMutable()
        {
            if (State is PromotionState.Completed or PromotionState.Cancelled)
                throw new DomainException($"Promotion is immutable in the {State} state.");
        }

        private static void EnsureValidEnvironmentProgression(DeploymentEnvironment source, DeploymentEnvironment target)
        {
            int sourceIndex = Array.IndexOf(EnvironmentProgression, source);
            int targetIndex = Array.IndexOf(EnvironmentProgression, target);

            if (targetIndex == -1)
                throw new DomainException($"Invalid target environment: {target}");
            
            if (sourceIndex == -1)
                throw new DomainException($"Invalid source environment: {source}");

            if (source == target)
                throw new DomainException("Source and target environments cannot be the same.");

            if (targetIndex == 0)
                throw new DomainException("Cannot promote TO the initial Dev environment.");

            if (targetIndex != sourceIndex + 1)
                throw new DomainException($"Cannot skip environments. {source} → {target} is not a valid progression.");
        }
    }
}