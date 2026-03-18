using ReleasePilot.Domain.Aggregates;

namespace ReleasePilot.Domain.Repositories
{
    public interface IPromotionRepository
    {
        Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default);
        Task UpdateAsync(Promotion promotion, CancellationToken cancellationToken = default);
    }
}