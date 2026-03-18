using MediatR;
using ReleasePilot.Domain.Enums;

namespace ReleasePilot.Application.Queries.ListPromotionsByApplication
{
    public record PromotionSummaryDto(
        Guid Id,
        string Version,
        DeploymentEnvironment SourceEnvironment,
        DeploymentEnvironment TargetEnvironment,
        PromotionState State,
        DateTimeOffset CreatedAt);

    public record PaginatedList<T>(
        IEnumerable<T> Items,
        int TotalCount,
        int PageIndex,
        int PageSize);

    public record ListPromotionsByApplicationQuery(
        string ApplicationId, 
        int PageIndex = 1, 
        int PageSize = 10) : IRequest<PaginatedList<PromotionSummaryDto>>;
}