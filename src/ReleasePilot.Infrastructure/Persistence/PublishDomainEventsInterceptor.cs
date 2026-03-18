using Microsoft.EntityFrameworkCore.Diagnostics;
using MassTransit;
using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Infrastructure.Persistence.Interceptors
{
    public class PublishDomainEventsInterceptor(IPublishEndpoint publishEndpoint) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            var aggregateRoots = context.ChangeTracker
                .Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var domainEvents = aggregateRoots
                .SelectMany(a => a.DomainEvents)
                .ToList();

            aggregateRoots.ForEach(a => a.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await publishEndpoint.Publish(
                    (object)domainEvent,
                    domainEvent.GetType(),
                    cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}