using Microsoft.EntityFrameworkCore;
using ReleasePilot.Domain.Aggregates;
using ReleasePilot.Domain.Repositories;

namespace ReleasePilot.Infrastructure.Persistence.Repositories
{
    public class PromotionRepository(ApplicationDbContext dbContext) : IPromotionRepository
    {
        public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await dbContext.Promotions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        
        public async Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
        {
            await dbContext.Promotions.AddAsync(promotion, cancellationToken);
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Promotion promotion, CancellationToken cancellationToken = default)
        {
            dbContext.Promotions.Update(promotion);
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}