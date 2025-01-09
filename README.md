EventFlow

Moongazing.EventFlow is a lightweight and extensible event bus library for .NET applications. It integrates seamlessly with RabbitMQ, enabling developers to build event-driven architectures with ease.
Features

    Publish and Subscribe: Publish events and subscribe to handlers using a straightforward API.
    RabbitMQ Integration: Built-in support for RabbitMQ as the message broker.
    Dependency Injection Friendly: Easily integrate into .NET projects with Microsoft DI.
    Asynchronous Consumer Support: Handles asynchronous message processing using RabbitMQ's async consumer capabilities.
    Extensible Design: Extend to other messaging platforms like Azure Service Bus or Kafka.

Installation

    Install the required NuGet packages:

    dotnet add package RabbitMQ.Client --version 7.0.0
    dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 9.0.0

    Add the Moongazing.EventFlow package to your solution (replace with your local library path or NuGet package).

Usage
1. Setup RabbitMQ

Start a RabbitMQ instance locally using Docker:

version: "3.8"
services:
  rabbitmq:
    image: rabbitmq:management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

2. Define an Event

Create a custom event by implementing the IEvent interface:

public class OrderCreatedEvent : IEvent
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

3. Create an Event Handler

Create an event handler by implementing the IEventHandler<TEvent> interface:

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event)
    {
        Console.WriteLine($"Order Created: {event.OrderId} at {event.CreatedAt}");
        return Task.CompletedTask;
    }
}

4. Register Dependencies

Use the provided AddEventBus extension method in your Startup.cs or Program.cs:

using Microsoft.Extensions.DependencyInjection;
using Moongazing.EventFlow;

var services = new ServiceCollection();

// Add RabbitMQ EventBus
services.AddEventBus("amqp://guest:guest@localhost:5672/");

// Register Event Handlers
services.AddTransient<OrderCreatedEventHandler>();

var serviceProvider = services.BuildServiceProvider();

5. Publish an Event

Use the IEventBus to publish an event:

var eventBus = serviceProvider.GetRequiredService<IEventBus>();

var orderEvent = new OrderCreatedEvent
{
    OrderId = Guid.NewGuid(),
    CreatedAt = DateTime.UtcNow
};

await eventBus.PublishAsync(orderEvent);

6. Subscribe and Listen

Subscribe to an event and start listening:

var eventBus = serviceProvider.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();

var rabbitMQEventBus = (RabbitMQEventBus)eventBus;
rabbitMQEventBus.StartListening();

Advanced Features
Extending to Other Message Brokers

You can implement your own message broker integration (e.g., Azure Service Bus) by extending the BaseEventBus class. The library is designed to support multiple platforms with minimal effort.
Requirements

    .NET 6 or later
    RabbitMQ instance (local or cloud-based)

Contributing

Contributions are welcome! To contribute:

    Fork the repository.
    Create a feature branch (git checkout -b feature-name).
    Commit your changes (git commit -m "Added feature").
    Push to the branch (git push origin feature-name).
    Open a pull request.

License

This project is licensed under the MIT License. See the LICENSE file for more details.
Issues

If you encounter any issues or have questions, feel free to open an issue in the repository or contact the maintainer.
