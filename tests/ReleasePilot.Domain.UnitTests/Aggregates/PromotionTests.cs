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
    private const string DefaultRequester = "user-requester";

    #region Request

    [Fact]
    public void Request_WithValidParameters_CreatesPromotionInRequestedState()
    {
        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging, DefaultRequester);

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

        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging, DefaultRequester, workItems);

        Assert.Equal(2, promotion.WorkItemReferences.Count);
    }

    [Fact]
    public void Request_WithoutWorkItems_HasEmptyWorkItemReferences()
    {
        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging, DefaultRequester);

        Assert.Empty(promotion.WorkItemReferences);
    }

    [Fact]
    public void Request_RaisesPromotionRequestedEvent()
    {
        var promotion = Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging, DefaultRequester);

        var domainEvent = Assert.Single(promotion.DomainEvents);
        var requested = Assert.IsType<PromotionRequested>(domainEvent);
        Assert.Equal(promotion.Id, requested.PromotionId);
        Assert.Equal(DefaultRequester, requested.ActingUser);
    }

    [Fact]
    public void Request_SameSourceAndTarget_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Dev, DefaultRequester));
    }

    [Fact]
    public void Request_TargetIsDev_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Staging, DeploymentEnvironment.Dev, DefaultRequester));
    }

    [Fact]
    public void Request_SkipsEnvironment_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Production, DefaultRequester));
    }

    [Theory]
    [InlineData(DeploymentEnvironment.Staging, DeploymentEnvironment.Dev)]
    [InlineData(DeploymentEnvironment.Production, DeploymentEnvironment.Staging)]
    public void Request_ReverseProgression_ThrowsDomainException(DeploymentEnvironment source, DeploymentEnvironment target)
    {
        Assert.Throws<DomainException>(() =>
            Promotion.Request(DefaultAppId, DefaultVersion, source, target, DefaultRequester));
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

        var evt = promotion.DomainEvents.OfType<PromotionApproved>().Single();
        Assert.Equal(DefaultUser, evt.ActingUser);
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

        promotion.StartDeployment(DefaultUser);

        Assert.Equal(PromotionState.InProgress, promotion.State);
    }

    [Fact]
    public void StartDeployment_RaisesDeploymentStartedEvent()
    {
        var promotion = CreateApprovedPromotion();

        promotion.StartDeployment(DefaultUser);

        var evt = promotion.DomainEvents.OfType<DeploymentStarted>().Single();
        Assert.Equal(DefaultUser, evt.ActingUser);
    }

    [Fact]
    public void StartDeployment_WhenRequested_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() => promotion.StartDeployment(DefaultUser));
    }

    [Fact]
    public void StartDeployment_WhenInProgress_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.StartDeployment(DefaultUser));
    }

    #endregion

    #region Complete

    [Fact]
    public void Complete_WhenInProgress_SetsCompletedState()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Complete(DefaultUser);

        Assert.Equal(PromotionState.Completed, promotion.State);
    }

    [Fact]
    public void Complete_RaisesPromotionCompletedEvent()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Complete(DefaultUser);

        var evt = promotion.DomainEvents.OfType<PromotionCompleted>().Single();
        Assert.Equal(DefaultUser, evt.ActingUser);
    }

    [Fact]
    public void Complete_WhenInProgress_SetsCompletedAtUtc()
    {
        var promotion = CreateInProgressPromotion();
        var before = DateTimeOffset.UtcNow;

        promotion.Complete(DefaultUser);

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

        Assert.Throws<DomainException>(() => promotion.Complete(DefaultUser));
    }

    [Fact]
    public void Complete_WhenApproved_ThrowsDomainException()
    {
        var promotion = CreateApprovedPromotion();

        Assert.Throws<DomainException>(() => promotion.Complete(DefaultUser));
    }

    #endregion

    #region Rollback

    [Fact]
    public void Rollback_WhenInProgress_SetsRolledBackState()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Rollback("Critical bug found", DefaultUser);

        Assert.Equal(PromotionState.RolledBack, promotion.State);
        Assert.Equal("Critical bug found", promotion.RollbackReason);
    }

    [Fact]
    public void Rollback_RaisesPromotionRolledBackEvent()
    {
        var promotion = CreateInProgressPromotion();

        promotion.Rollback("Reason", DefaultUser);

        var evt = promotion.DomainEvents.OfType<PromotionRolledBack>().Single();
        Assert.Equal("Reason", evt.Reason);
        Assert.Equal(DefaultUser, evt.ActingUser);
    }

    [Fact]
    public void Rollback_WithEmptyReason_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("", DefaultUser));
    }

    [Fact]
    public void Rollback_WithWhitespaceReason_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("   ", DefaultUser));
    }

    [Fact]
    public void Rollback_WhenRequested_ThrowsDomainException()
    {
        var promotion = CreateRequestedPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("Reason", DefaultUser));
    }

    [Fact]
    public void Rollback_WhenCompleted_ThrowsDomainException()
    {
        var promotion = CreateCompletedPromotion();

        Assert.Throws<DomainException>(() => promotion.Rollback("Reason", DefaultUser));
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_WhenRequested_SetsCancelledState()
    {
        var promotion = CreateRequestedPromotion();

        promotion.Cancel(DefaultRequester);

        Assert.Equal(PromotionState.Cancelled, promotion.State);
    }

    [Fact]
    public void Cancel_RaisesPromotionCancelledEvent()
    {
        var promotion = CreateRequestedPromotion();

        promotion.Cancel(DefaultRequester);

        var evt = promotion.DomainEvents.OfType<PromotionCancelled>().Single();
        Assert.Equal(DefaultRequester, evt.ActingUser);
    }

    [Fact]
    public void Cancel_WhenApproved_ThrowsDomainException()
    {
        var promotion = CreateApprovedPromotion();

        Assert.Throws<DomainException>(() => promotion.Cancel(DefaultRequester));
    }

    [Fact]
    public void Cancel_WhenInProgress_ThrowsDomainException()
    {
        var promotion = CreateInProgressPromotion();

        Assert.Throws<DomainException>(() => promotion.Cancel(DefaultRequester));
    }

    [Fact]
    public void Cancel_WhenCompleted_ThrowsDomainException()
    {
        var promotion = CreateCompletedPromotion();

        Assert.Throws<DomainException>(() => promotion.Cancel(DefaultRequester));
    }

    #endregion

    #region Helpers

    private static Promotion CreateRequestedPromotion()
        => Promotion.Request(DefaultAppId, DefaultVersion, DeploymentEnvironment.Dev, DeploymentEnvironment.Staging, DefaultRequester);

    private static Promotion CreateApprovedPromotion()
    {
        var promotion = CreateRequestedPromotion();
        promotion.Approve(DefaultUser, new StubAuthorizationPolicy(true), new StubConcurrencyPolicy(false));
        return promotion;
    }

    private static Promotion CreateInProgressPromotion()
    {
        var promotion = CreateApprovedPromotion();
        promotion.StartDeployment(DefaultUser);
        return promotion;
    }

    private static Promotion CreateCompletedPromotion()
    {
        var promotion = CreateInProgressPromotion();
        promotion.Complete(DefaultUser);
        return promotion;
    }

    private static Promotion CreateCancelledPromotion()
    {
        var promotion = CreateRequestedPromotion();
        promotion.Cancel(DefaultRequester);
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