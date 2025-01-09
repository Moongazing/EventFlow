using Moongazing.EventFlow.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Moongazing.EventFlow.Kernel;

public abstract class BaseEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, Type> handlers = new();

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        handlers.TryAdd(typeof(TEvent), typeof(THandler));
    }

    protected Type GetHandlerForEvent(Type eventType)
    {
        return handlers.TryGetValue(eventType, out var handlerType) ? handlerType : null!;
    }

    public abstract Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
}