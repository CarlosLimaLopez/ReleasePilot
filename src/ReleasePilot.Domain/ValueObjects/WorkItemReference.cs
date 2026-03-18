using System.Text.Json.Serialization;
using ReleasePilot.Domain.Exceptions;

namespace ReleasePilot.Domain.ValueObjects
{
    public record WorkItemReference
    {
        public string IssueId { get; init; }

        [JsonConstructor]
        public WorkItemReference(string issueId)
        {
            IssueId = issueId;
        }

        public static WorkItemReference Create(string issueId)
        {
            if (string.IsNullOrWhiteSpace(issueId))
                throw new DomainException("Work item issue ID is required.");
            
            return new WorkItemReference(issueId);
        }
    }
}