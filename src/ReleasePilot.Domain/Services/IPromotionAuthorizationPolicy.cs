namespace ReleasePilot.Domain.Services
{
    public interface IPromotionAuthorizationPolicy
    {
        /// <summary>
        /// Evaluates if the specified user has the required Approver role for a promotion.
        /// </summary>
        /// <param name="userId">The identifier of the user attempting to approve.</param>
        /// <returns>True if the user has the Approver role; otherwise, false.</returns>
        bool HasApproverRole(string userId);
    }
}