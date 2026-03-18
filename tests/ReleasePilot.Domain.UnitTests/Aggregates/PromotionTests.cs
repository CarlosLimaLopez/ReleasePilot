using ReleasePilot.Domain.Aggregates;
using ReleasePilot.Domain.Enums;
using ReleasePilot.Domain.Events;
using ReleasePilot.Domain.Exceptions;
using ReleasePilot.Domain.Services;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Domain.UnitTests.Aggregates;

public class PromotionTests
{
    private const string DefaultAppId = "app-1";
    private const string DefaultVersion = "1.0.0";
    private const string DefaultUser = "user-approver";

    #region Request

    [Fact]
    public void Request_WithValidParameters_CreatesPromotionInRequestedState()
    {
        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging);

        Assert.NotEqual(Guid.Empty, promotion.Id);
        Assert.Equal(DefaultAppId, promotion.ApplicationId);
        Assert.Equal(DefaultVersion, promotion.Version.Value);
        Assert.Equal(DeploymentEnvironment.Dev, promotion.SourceEnvironment);
        Assert.Equal(DeploymentEnvironment.Staging, promotion.TargetEnvironment);
        Assert.Equal(PromotionState.Requested, promotion.State);
        Assert.Null(promotion.RollbackReason);
    }

    [Fact]
    public void Request_WithWorkItems_IncludesWorkItemReferences()
    {
        var workItems = new[] { WorkItemReference.Create("WI-1"), WorkItemReference.Create("WI-2") };

        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging, workItems);

        Assert.Equal(2, promotion.WorkItemReferences.Count);
    }

    [Fact]
    public void Request_WithoutWorkItems_HasEmptyWorkItemReferences()
    {
        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging);

        Assert.Empty(promotion.WorkItemReferences);
    }

    [Fact]
    public void Request_RaisesPromotionRequestedEvent()
    {
        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging);

        var domainEvent = Assert.Single(promotion.DomainEvents);
        var requested = Assert.IsType<PromotionRequested>(domainEvent);
        Assert.Equal(promotion.Id, requested.PromotionId);
    }

    [Fact]
    public void Request_SameSourceAndTarget_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Dev));
    }

    [Fact]
    public void Request_TargetIsDev_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Staging, DeploymentEnvironment.Dev));
    }

    [Fact]
    public void Request_SkipsEnvironment_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Production));
    }

    [Theory]
    [InlineData(DeploymentEnvironment.Staging, DeploymentEnvironment.Dev)]
    [InlineData(DeploymentEnvironment.Production, DeploymentEnvironment.Staging)]
    public void Request_ReverseProgression_ThrowsDomainException(DeploymentEnvironment source, DeploymentEnvironment target)
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, source, target));
    }

    #endregion

    #region Approve

    [Fact]
    public void Approve_WhenRequestedAndAuthorized_SetsApprovedState()
    {
        var promotion = CreateRequestedPromotion();

        promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false));

        Assert.Equal(PromotionState.Approved, promotion.State);
    }

    [Fact]
    public void Approve_RaisesPromotionApprovedEvent()
    {
        var promotion = CreateRequestedPromotion();

        promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false));

        Assert.Contains(promotion.DomainEvents, e => e is PromotionApproved);
    }

    [Fact]
    public void Approve_UserWithoutApproverRole_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() =>
            promotion.Approve("non-approver", new StubAuthorizationPolicy(false), new StubConcurrencyPolicy(false)));
    }

    [Fact]
    public void Approve_WhenConcurrentPromotionExists_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() =>
            promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(true)));
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ThrowsDomainException()
    {
        var promotion = CreateApprovedPromotion();

        Assert.Throws<DomainException>(() =>
            promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false)));
    }

    [Fact]
    public void Approve_WhenCompleted_ThrowsDomainException()
    {
        var promotion = CreateCompletedPromotion();

        Assert.Throws<DomainException>(() =>
            promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false)));
    }

    [Fact]
    public void Approve_WhenCancelled_ThrowsDomainException()
    {
        var promotion = CreateCancelledPromotion();

        Assert.Throws<DomainException>(() =>
            promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false)));
    }

    #endregion

    #region StartDeployment

    [Fact]
    public void StartDeployment_WhenApproved_SetsInProgressState()
    {
        var promotion = CreateApprovedPromotion();

        promotion.StartDeployment();

        Assert.Equal(PromotionState.InProgress, promotion.State);
    }

    [Fact]
    public void StartDeployment_RaisesDeploymentStartedEvent()
    {
        var promotion = CreateApprovedPromotion();

        promotion.StartDeployment();

        Assert.Contains(promotion.DomainEvents, e => e is DeploymentStarted);
    }

    [Fact]
    public void StartDeployment_WhenRequested_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() => promotion.StartDeployment());
    }

    [Fact]
    public void StartDeployment_WhenInProgress_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.StartDeployment());
    }

    #endregion

    #region Complete

    [Fact]
    public void Complete_WhenInProgress_SetsCompletedState()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Complete();

        Assert.Equal(PromotionState.Completed, promotion.State);
    }

    [Fact]
    public void Complete_RaisesPromotionCompletedEvent()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Complete();

        Assert.Contains(promotion.DomainEvents, e => e is PromotionCompleted);
    }

    [Fact]
    public void Complete_WhenInProgress_SetsCompletedAtUtc()
    {
        var promotion = CreateInProgressPromotion();
        var before = DateTimeOffset.UtcNow;

        promotion.Complete();

        var after = DateTimeOffset.UtcNow;
        Assert.NotNull(promotion.CompletedAtUtc);
        Assert.InRange(promotion.CompletedAtUtc.Value, before, after);
    }

    [Fact]
    public void Request_CompletedAtUtc_IsNull()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Null(promotion.CompletedAtUtc);
    }

    [Fact]
    public void Complete_WhenRequested_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() => promotion.Complete());
    }

    [Fact]
    public void Complete_WhenApproved_ThrowsDomainException()
    {
        var promotion = CreateApprovedPromotion();

        Assert.Throws<DomainException>(() => promotion.Complete());
    }

    #endregion

    #region Rollback

    [Fact]
    public void Rollback_WhenInProgress_SetsRolledBackState()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Rollback("Critical bug found");

        Assert.Equal(PromotionState.RolledBack, promotion.State);
        Assert.Equal("Critical bug found", promotion.RollbackReason);
    }

    [Fact]
    public void Rollback_RaisesPromotionRolledBackEvent()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Rollback("Reason");

        var evt = promotion.DomainEvents.OfType<PromotionRolledBack>().Single();
        Assert.Equal("Reason", evt.Reason);
    }

    [Fact]
    public void Rollback_WithEmptyReason_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback(""));
    }

    [Fact]
    public void Rollback_WithWhitespaceReason_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("   "));
    }

    [Fact]
    public void Rollback_WhenRequested_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("Reason"));
    }

    [Fact]
    public void Rollback_WhenCompleted_ThrowsDomainException()
    {
        var promotion = CreateCompletedPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("Reason"));
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_WhenRequested_SetsCancelledState()
    {
        var promotion = CreateRequestedPromotion();

        promotion.Cancel();

        Assert.Equal(PromotionState.Cancelled, promotion.State);
    }

    [Fact]
    public void Cancel_RaisesPromotionCancelledEvent()
    {
        var promotion = CreateRequestedPromotion();

        promotion.Cancel();

        Assert.Contains(promotion.DomainEvents, e => e is PromotionCancelled);
    }

    [Fact]
    public void Cancel_WhenApproved_ThrowsDomainException()
    {
        var promotion = CreateApprovedPromotion();

        Assert.Throws<DomainException>(() => promotion.Cancel());
    }

    [Fact]
    public void Cancel_WhenInProgress_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.Cancel());
    }

    [Fact]
    public void Cancel_WhenCompleted_ThrowsDomainException()
    {
        var promotion = CreateCompletedPromotion();

        Assert.Throws<DomainException>(() => promotion.Cancel());
    }

    #endregion

    #region Helpers

    private static Promotion CreateRequestedPromotion()
        => Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging);

    private static Promotion CreateApprovedPromotion()
    {
        var promotion = CreateRequestedPromotion();
        promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false));
        return promotion;
    }

    private static Promotion CreateInProgressPromotion()
    {
        var promotion = CreateApprovedPromotion();
        promotion.StartDeployment();
        return promotion;
    }

    private static Promotion CreateCompletedPromotion()
    {
        var promotion = CreateInProgressPromotion();
        promotion.Complete();
        return promotion;
    }

    private static Promotion CreateCancelledPromotion()
    {
        var promotion = CreateRequestedPromotion();
        promotion.Cancel();
        return promotion;
    }

    private sealed class StubAuthorizationPolicy(bool hasRole) : IPromotionAuthorizationPolicy
    {
        public bool HasApproverRole(string userId) => hasRole;
    }

    private sealed class StubConcurrencyPolicy(bool hasExisting) : IPromotionConcurrencyPolicy
    {
        public bool HasExistingInProgress(string applicationId, DeploymentEnvironment targetEnvironment) => hasExisting;
    }

    #endregion
}