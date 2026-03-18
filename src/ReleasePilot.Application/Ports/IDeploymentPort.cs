using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Application.Ports
{
    public interface IDeploymentPort
    {
        /// <summary>
        /// Initiates an asynchronous deployment process based on the specified promotion details.
        /// </summary>

        /// <param name="promotion">The promotion object that contains the information required to start the deployment. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the deployment operation.</param>
        /// <returns>A task that represents the asynchronous operation of triggering the deployment.</returns>
        Task TriggerDeploymentAsync(Promotion promotion, CancellationToken cancellationToken = default);
    }
}