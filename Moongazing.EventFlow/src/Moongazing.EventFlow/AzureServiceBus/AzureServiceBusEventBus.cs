using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using Moongazing.EventFlow.Kernel;
using System.Collections.Concurrent;

namespace Moongazing.EventFlow.AzureServiceBus;

public class AzureServiceBusEventBus : BaseEventBus
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly IServiceProvider serviceProvider;
    private readonly ConcurrentDictionary<string, ServiceBusProcessor> processors;

    public AzureServiceBusEventBus(ServiceBusClient serviceBusClient, IServiceProvider serviceProvider)
    {
        this.serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        processors = new ConcurrentDictionary<string, ServiceBusProcessor>();
    }

    public override async Task PublishAsync<TEvent>(TEvent @event)
    {
        var queueName = typeof(TEvent).Name;
        var sender = serviceBusClient.CreateSender(queueName);

        try
        {
            var messageBody = JsonSerializer.Serialize(@event);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));

            await sender.SendMessageAsync(message);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public void StartListening()
    {
        foreach (var eventType in GetHandlers().Keys)
        {
            var queueName = eventType.Name;

            if (!processors.ContainsKey(queueName))
            {
                var processor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = true
                });

                processor.ProcessMessageAsync += async args =>
                {
                    var messageBody = Encoding.UTF8.GetString(args.Message.Body);
                    var eventType = GetHandlers().Keys.FirstOrDefault(x => x.Name == queueName);
                    if (eventType == null) return;

                    var @event = JsonSerializer.Deserialize(messageBody, eventType);
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

                processor.ProcessErrorAsync += args =>
                {
                    Console.WriteLine($"Error occurred: {args.Exception.Message}");
                    return Task.CompletedTask;
                };

                processors.TryAdd(queueName, processor);
                processor.StartProcessingAsync();
            }
        }
    }

    public async Task StopListeningAsync()
    {
        foreach (var processor in processors.Values)
        {
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }

        processors.Clear();
    }

    private ConcurrentDictionary<Type, Type> GetHandlers()
    {
        return (ConcurrentDictionary<Type, Type>)typeof(BaseEventBus)
            .GetField("_handlers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(this)!;
    }
}
