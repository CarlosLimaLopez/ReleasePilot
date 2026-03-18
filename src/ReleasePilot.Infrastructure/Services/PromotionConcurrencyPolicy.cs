using ReleasePilot.Domain.Enums;
using ReleasePilot.Domain.Services;
using ReleasePilot.Infrastructure.Persistence;

namespace ReleasePilot.Infrastructure.Services;

public class PromotionConcurrencyPolicy(ApplicationDbContext dbContext) : IPromotionConcurrencyPolicy
{
    public bool HasExistingInProgress(string applicationId, DeploymentEnvironment targetEnvironment)
        => dbContext.Promotions.Any(p =>
            p.ApplicationId == applicationId &&
            p.TargetEnvironment == targetEnvironment &&
            p.State == PromotionState.InProgress);
}