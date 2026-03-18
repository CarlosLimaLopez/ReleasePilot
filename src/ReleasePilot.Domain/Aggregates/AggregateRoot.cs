using ReleasePilot.Domain.Events;

namespace ReleasePilot.Domain.Aggregates
{
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}