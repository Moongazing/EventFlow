using Moongazing.EventFlow.Interfaces;

namespace Moongazing.EventFlow.Events;

public class OrderCreatedEvent : IEvent
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
