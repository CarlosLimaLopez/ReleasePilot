using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Application.Ports
{
    public interface IApplicationDbContext
    {
        IQueryable<Promotion> Promotions { get; }
        IQueryable<AuditLog> AuditLogs { get; }
    }
}