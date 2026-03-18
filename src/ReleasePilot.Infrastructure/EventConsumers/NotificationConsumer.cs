using System.Threading.Tasks;
using MassTransit;
using ReleasePilot.Application.Ports;
using ReleasePilot.Domain.Events;
using ReleasePilot.Domain.Repositories;

namespace ReleasePilot.Infrastructure.EventConsumers;

public class NotificationConsumer(
    IPromotionRepository repository,
    INotificationPort notificationPort) :
    IConsumer<PromotionCompleted>,
    IConsumer<PromotionRolledBack>,
    IConsumer<PromotionCancelled>
{
    public async Task Consume(ConsumeContext<PromotionCompleted> context)
        => await NotifyAsync(context.Message.PromotionId);

    public async Task Consume(ConsumeContext<PromotionRolledBack> context)
        => await NotifyAsync(context.Message.PromotionId);

    public async Task Consume(ConsumeContext<PromotionCancelled> context)
        => await NotifyAsync(context.Message.PromotionId);

    private async Task NotifyAsync(System.Guid promotionId)
    {
        var promotion = await repository.GetByIdAsync(promotionId);
        if (promotion is null) return;

        await notificationPort.NotifyTerminalStateReachedAsync(promotion);
    }
}