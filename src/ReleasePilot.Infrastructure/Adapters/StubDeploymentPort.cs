using Microsoft.Extensions.Logging;
using ReleasePilot.Application.Ports;
using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Infrastructure.Adapters;

public class StubDeploymentPort(ILogger<StubDeploymentPort> logger) : IDeploymentPort
{
    public Task TriggerDeploymentAsync(Promotion promotion, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[Deployment] Triggered deployment for Promotion {PromotionId}: '{AppId}' v{Version} → {Target}",
            promotion.Id,
            promotion.ApplicationId,
            promotion.Version,
            promotion.TargetEnvironment);

        return Task.CompletedTask;
    }
}