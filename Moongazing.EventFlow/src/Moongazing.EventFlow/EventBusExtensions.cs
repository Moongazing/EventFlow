using Microsoft.Extensions.DependencyInjection;
using Moongazing.EventFlow.Interfaces;
using Moongazing.EventFlow.RabbitMq;
using RabbitMQ.Client;

namespace Moongazing.EventFlow
{
    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, string rabbitMqConnectionString)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqConnectionString),
                DispatchConsumersAsync = true 
            };

            var connection = connectionFactory.CreateConnection();
            services.AddSingleton(connection);

            services.AddSingleton<IEventBus, RabbitMQEventBus>();

            return services;
        }
    }
}
