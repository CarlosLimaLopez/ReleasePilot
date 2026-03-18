using ReleasePilot.Domain.Enums;

namespace ReleasePilot.Domain.Services
{
    public interface IPromotionConcurrencyPolicy
    {
        /// <summary>
        /// Checks if there is an existing promotion for the same application and target environment that is currently in progress.
        /// </summary>
        /// <param name="applicationId">The identifier of the application being promoted.</param>
        /// <param name="targetEnvironment">The target environment for the promotion.</param>
        /// <returns>True if a promotion is already in progress; otherwise, false.</returns>
        bool HasExistingInProgress(string applicationId, DeploymentEnvironment targetEnvironment);
    }
}