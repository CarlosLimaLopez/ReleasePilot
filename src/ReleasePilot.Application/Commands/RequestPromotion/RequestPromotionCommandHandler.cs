using MediatR;
using ReleasePilot.Domain.Aggregates;
using ReleasePilot.Domain.Repositories;
using ReleasePilot.Domain.ValueObjects;

namespace ReleasePilot.Application.Commands.RequestPromotion
{
    public class RequestPromotionCommandHandler(
        IPromotionRepository repository) 
        : IRequestHandler<RequestPromotionCommand, Guid>
    {
        public async Task<Guid> Handle(RequestPromotionCommand request, CancellationToken cancellationToken)
        {            
            var workItems = request.WorkItems?
                .Select(w => WorkItemReference.Create(w.IssueId));

            var promotion = Promotion.Request(
                request.ApplicationId,
                request.Version,
                request.SourceEnvironment,
                request.TargetEnvironment,
                workItems);

            await repository.AddAsync(promotion, cancellationToken);

            return promotion.Id;
        }
    }
}