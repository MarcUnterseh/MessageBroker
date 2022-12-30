using MessageBroker.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.RabbitMQ.RabbitMQ;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMQConfiguration = new RabbitMQConfiguration(configuration);
        services.AddSingleton(rabbitMQConfiguration);
        
        services.AddSingleton<IClient, RabbitMQClient>();
        services.AddHostedService<RabbitMQConnector>();
        
        services.AddSingleton<IEventPublisher, RabbitMQPublisher>();
        services.AddSingleton<ISubscriber, RabbitMQSubscriber>();

        return services;
    }
}