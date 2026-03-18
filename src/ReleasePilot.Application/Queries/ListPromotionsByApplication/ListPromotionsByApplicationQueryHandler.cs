using MediatR;
using Microsoft.EntityFrameworkCore;
using ReleasePilot.Application.Ports;

namespace ReleasePilot.Application.Queries.ListPromotionsByApplication
{
    public class ListPromotionsByApplicationQueryHandler(IApplicationDbContext dbContext) 
        : IRequestHandler<ListPromotionsByApplicationQuery, PaginatedList<PromotionSummaryDto>>
    {
        public async Task<PaginatedList<PromotionSummaryDto>> Handle(ListPromotionsByApplicationQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = dbContext.Promotions
                .AsNoTracking()
                .Where(p => p.ApplicationId == request.ApplicationId);

            int totalCount = await baseQuery.CountAsync(cancellationToken);

            var pagedItems = await baseQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PromotionSummaryDto(
                    p.Id,
                    p.Version.Value,
                    p.SourceEnvironment,
                    p.TargetEnvironment,
                    p.State,
                    p.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedList<PromotionSummaryDto>(
                Items: pagedItems, 
                TotalCount: totalCount, 
                PageIndex: request.PageIndex, 
                PageSize: request.PageSize);
        }
    }
}