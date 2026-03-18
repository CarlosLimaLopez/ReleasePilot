using MediatR;
using Microsoft.EntityFrameworkCore;
using ReleasePilot.Application.Ports;

namespace ReleasePilot.Application.Queries.GetPromotionById
{
    public class GetPromotionByIdQueryHandler(
        IApplicationDbContext dbContext,
        IIssueTrackerPort issueTrackerPort) 
        : IRequestHandler<GetPromotionByIdQuery, PromotionDetailsDto?>
    {
        public async Task<PromotionDetailsDto?> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
        {
            var promotionProjection = await dbContext.Promotions
                .AsNoTracking()
                .Where(p => p.Id == request.PromotionId)
                .Select(p => new
                {
                    p.Id,
                    p.ApplicationId,
                    Version = p.Version.Value,
                    p.SourceEnvironment,
                    p.TargetEnvironment,
                    p.State,
                    p.RollbackReason,
                    p.WorkItemReferences
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (promotionProjection == null)
            {
                return null;
            }

            var enrichedWorkItems = new List<WorkItemDetailsDto>();
            
            if (promotionProjection.WorkItemReferences.Count != 0)
            {
                var workItemDetails = await issueTrackerPort.GetWorkItemsInformationAsync(
                    promotionProjection.WorkItemReferences, 
                    cancellationToken);

                enrichedWorkItems = workItemDetails
                    .Select(w => new WorkItemDetailsDto(w.Id, w.Title, w.Status))
                    .ToList();
            }

            return new PromotionDetailsDto(
                Id: promotionProjection.Id,
                ApplicationId: promotionProjection.ApplicationId,
                Version: promotionProjection.Version,
                SourceEnvironment: promotionProjection.SourceEnvironment,
                TargetEnvironment: promotionProjection.TargetEnvironment,
                State: promotionProjection.State,
                RollbackReason: promotionProjection.RollbackReason,
                WorkItems: enrichedWorkItems
            );
        }
    }
}