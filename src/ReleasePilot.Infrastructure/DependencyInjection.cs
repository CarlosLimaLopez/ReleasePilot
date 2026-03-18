using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReleasePilot.Application.Ports;
using ReleasePilot.Domain.Repositories;
using ReleasePilot.Domain.Services;
using ReleasePilot.Infrastructure.Adapters;
using ReleasePilot.Infrastructure.EventConsumers;
using ReleasePilot.Infrastructure.Persistence;
using ReleasePilot.Infrastructure.Persistence.Interceptors;
using ReleasePilot.Infrastructure.Persistence.Repositories;
using ReleasePilot.Infrastructure.Services;

namespace ReleasePilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<PublishDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options
                .UseNpgsql(connectionString)
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IPromotionRepository, PromotionRepository>();

        // Domain service policies
        services.AddScoped<IPromotionAuthorizationPolicy, PromotionAuthorizationPolicy>();
        services.AddScoped<IPromotionConcurrencyPolicy, PromotionConcurrencyPolicy>();

        // External system ports (stubs)
        services.AddScoped<INotificationPort, StubNotificationPort>();
        services.AddScoped<IDeploymentPort, StubDeploymentPort>();
        services.AddScoped<IIssueTrackerPort, StubIssueTrackerPort>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<AuditLogConsumer>();
            x.AddConsumer<NotificationConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMq:Host"] ?? "localhost";
                var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";

                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}