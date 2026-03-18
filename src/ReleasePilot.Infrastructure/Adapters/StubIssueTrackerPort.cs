using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReleasePilot.Application.Ports;
using ReleasePilot.Application.Ports.Models;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Infrastructure.Adapters;

public class StubIssueTrackerPort(ILogger<StubIssueTrackerPort> logger) : IIssueTrackerPort
{
    public Task<IEnumerable<WorkItemDetails>> GetWorkItemsInformationAsync(
        IEnumerable<WorkItemReference> references,
        CancellationToken cancellationToken = default)
    {
        var results = references.Select(r =>
        {
            logger.LogInformation("[IssueTracker] Fetching work item {IssueId}", r.IssueId);

            return new WorkItemDetails(
                Id: r.IssueId,
                Title: $"Work item {r.IssueId}",
                Description: $"Stub description for {r.IssueId}",
                Status: "Done");
        });

        return Task.FromResult(results);
    }
}