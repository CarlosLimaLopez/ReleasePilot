using ReleasePilot.Domain.Services;

namespace ReleasePilot.Infrastructure.Services;

public class PromotionAuthorizationPolicy : IPromotionAuthorizationPolicy
{
    private static readonly HashSet<string> Approvers = new(StringComparer.OrdinalIgnoreCase)
    {
        "approver-1",
        "approver-2",
        "admin"
    };

    public bool HasApproverRole(string userId)
        => Approvers.Contains(userId);
}