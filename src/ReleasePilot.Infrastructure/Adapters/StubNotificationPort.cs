using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReleasePilot.Application.Ports;
using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Infrastructure.Adapters;

public class StubNotificationPort(ILogger<StubNotificationPort> logger) : INotificationPort
{
    public Task NotifyTerminalStateReachedAsync(Promotion promotion, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[Notification] Promotion {PromotionId} for '{AppId}' v{Version} reached terminal state: {State}",
            promotion.Id,
            promotion.ApplicationId,
            promotion.Version,
            promotion.State);

        return Task.CompletedTask;
    }
}