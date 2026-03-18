using System;
using System.Threading.Tasks;
using MassTransit;
using ReleasePilot.Domain.Aggregates;
using ReleasePilot.Domain.Events;
using ReleasePilot.Infrastructure.Persistence;

namespace ReleasePilot.Infrastructure.EventConsumers;

public class AuditLogConsumer(ApplicationDbContext dbContext) :
    IConsumer<PromotionRequested>,
    IConsumer<PromotionApproved>,
    IConsumer<DeploymentStarted>,
    IConsumer<PromotionCompleted>,
    IConsumer<PromotionRolledBack>,
    IConsumer<PromotionCancelled>
{
    public Task Consume(ConsumeContext<PromotionRequested> context)
        => LogEventAsync(context.Message.PromotionId, nameof(PromotionRequested), context.Message.ActingUser);

    public Task Consume(ConsumeContext<PromotionApproved> context)
        => LogEventAsync(context.Message.PromotionId, nameof(PromotionApproved), context.Message.ActingUser);

    public Task Consume(ConsumeContext<DeploymentStarted> context)
        => LogEventAsync(context.Message.PromotionId, nameof(DeploymentStarted), context.Message.ActingUser);

    public Task Consume(ConsumeContext<PromotionCompleted> context)
        => LogEventAsync(context.Message.PromotionId, nameof(PromotionCompleted), context.Message.ActingUser);

    public Task Consume(ConsumeContext<PromotionRolledBack> context)
        => LogEventAsync(context.Message.PromotionId, nameof(PromotionRolledBack), context.Message.ActingUser);

    public Task Consume(ConsumeContext<PromotionCancelled> context)
        => LogEventAsync(context.Message.PromotionId, nameof(PromotionCancelled), context.Message.ActingUser);

    private async Task LogEventAsync(Guid promotionId, string eventType, string actingUser)
    {
        var log = AuditLog.Create(promotionId, eventType, DateTimeOffset.UtcNow, actingUser);
        
        await dbContext.AuditLogs.AddAsync(log);
        await dbContext.SaveChangesAsync();
    }
}