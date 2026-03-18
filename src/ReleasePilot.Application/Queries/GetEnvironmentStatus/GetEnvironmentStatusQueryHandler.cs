using MediatR;
using Microsoft.EntityFrameworkCore;
using ReleasePilot.Application.Ports;
using ReleasePilot.Domain.Enums;

namespace ReleasePilot.Application.Queries.GetEnvironmentStatus
{
    public class GetEnvironmentStatusQueryHandler(IApplicationDbContext dbContext) 
        : IRequestHandler<GetEnvironmentStatusQuery, IEnumerable<EnvironmentStatusDto>>
    {
        public async Task<IEnumerable<EnvironmentStatusDto>> Handle(GetEnvironmentStatusQuery request, CancellationToken cancellationToken)
        {
            var relevantPromotions = await dbContext.Promotions
                .AsNoTracking()
                .Where(p => p.ApplicationId == request.ApplicationId &&
                            (p.State == PromotionState.Completed || p.State == PromotionState.InProgress))
                .Select(p => new
                {
                    p.Id,
                    p.TargetEnvironment,
                    Version = p.Version.Value,
                    p.State,
                    p.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var dashboardSummary = relevantPromotions
                .GroupBy(p => p.TargetEnvironment)
                .Select(group => 
                {
                    var latestPromotion = group.OrderByDescending(p => p.CreatedAt).First();

                    return new EnvironmentStatusDto(
                        Environment: latestPromotion.TargetEnvironment,
                        CurrentPromotionId: latestPromotion.Id,
                        Version: latestPromotion.Version,
                        State: latestPromotion.State
                    );
                })
                .OrderBy(e => e.Environment)
                .ToList();

            var allEnvironments = Enum.GetValues<DeploymentEnvironment>();
            var finalResult = new List<EnvironmentStatusDto>();

            foreach (var env in allEnvironments)
            {
                var existingStatus = dashboardSummary.SingleOrDefault(s => s.Environment == env);
                
                if (existingStatus != null)
                    finalResult.Add(existingStatus);
                else
                    finalResult.Add(new EnvironmentStatusDto(env, Guid.Empty, "None", default));
            }

            return finalResult;
        }
    }
}