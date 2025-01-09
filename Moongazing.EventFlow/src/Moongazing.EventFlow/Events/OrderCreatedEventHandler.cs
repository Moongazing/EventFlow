using Moongazing.EventFlow.Interfaces;

using System.Threading.Tasks;
namespace Moongazing.EventFlow.Events;

public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event)
    {
        Console.WriteLine($"Order Created: {@event.OrderId} at {@event.CreatedAt}");
        return Task.CompletedTask;
    }
}
