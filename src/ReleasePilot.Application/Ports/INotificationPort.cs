using System.Threading;
using System.Threading.Tasks;
using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Application.Ports
{
    public interface INotificationPort
    {
        /// <summary>
        /// Sends notifications to stakeholders when a promotion reaches a terminal state
        /// </summary>
        Task NotifyTerminalStateReachedAsync(Promotion promotion, CancellationToken cancellationToken = default);
    }
}