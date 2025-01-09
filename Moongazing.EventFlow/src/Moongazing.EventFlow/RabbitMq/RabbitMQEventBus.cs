using Moongazing.EventFlow.Kernel;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Moongazing.EventFlow.RabbitMq;

public class RabbitMQEventBus : BaseEventBus
{
    private readonly IConnection connection;
    private readonly IServiceProvider serviceProvider;

    public RabbitMQEventBus(IConnection connection, IServiceProvider serviceProvider)
    {
        this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override Task PublishAsync<TEvent>(TEvent @event)
    {
        using var channel = connection.CreateModel();
        var queueName = typeof(TEvent).Name;

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public void StartListening()
    {
        var channel = connection.CreateModel();

        foreach (var eventType in GetHandlers().Keys)
        {
            var queueName = eventType.Name;

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var eventType = GetHandlers().Keys.FirstOrDefault(x => x.Name == queueName);
                if (eventType == null) return;

                var @event = JsonSerializer.Deserialize(message, eventType);
                var handlerType = GetHandlerForEvent(eventType);

                if (handlerType != null)
                {
                    var handler = serviceProvider.GetService(handlerType);
                    if (handler is not null)
                    {
                        var method = handlerType.GetMethod("HandleAsync");
                        if (method != null)
                        {
                            await (Task)method.Invoke(handler, [@event!])!;
                        }
                    }
                }
            };

            channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumer: consumer);
        }
    }

    private ConcurrentDictionary<Type, Type> GetHandlers()
    {
        return (ConcurrentDictionary<Type, Type>)typeof(BaseEventBus)
            .GetField("_handlers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(this)!;
    }
}
