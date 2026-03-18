using System;

namespace ReleasePilot.Domain.Aggregates;

public class AuditLog
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public Guid PromotionId { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string ActingUser { get; private set; }

    private AuditLog()
    {
        EventType = string.Empty;
        ActingUser = string.Empty;
    }

    public static AuditLog Create(Guid promotionId, string eventType, DateTimeOffset timestamp, string actingUser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(actingUser);
        
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            PromotionId = promotionId,
            EventType = eventType,
            Timestamp = timestamp,
            ActingUser = actingUser
        };
    }
}