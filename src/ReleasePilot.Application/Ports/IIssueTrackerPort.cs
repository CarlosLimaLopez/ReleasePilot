using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReleasePilot.Application.Ports.Models;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Application.Ports
{
    public interface IIssueTrackerPort
    {
        /// <summary>
        /// Gets detailed information about work items based on their references. This method is designed to fetch comprehensive details such as title, description, and status for each work item reference provided.
        /// </summary>
        /// <param name="references">A collection of work item references for which detailed information is requested.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of work item details.</returns>
        Task<IEnumerable<WorkItemDetails>> GetWorkItemsInformationAsync(
            IEnumerable<WorkItemReference> references, 
            CancellationToken cancellationToken = default);
    }
}