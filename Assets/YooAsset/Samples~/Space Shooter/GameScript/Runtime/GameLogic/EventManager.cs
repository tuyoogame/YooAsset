using System;
using Framework.Event;

/// <summary>
/// Shared event bus for the demo.
/// </summary>
public sealed class EventManager
{
    private static EventManager s_instance;
    public static EventManager Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new EventManager();
            return s_instance;
        }
    }

    private EventBus _bus;

    private EventManager()
    {
    }

    public void Create()
    {
        if (_bus != null)
            throw new InvalidOperationException("Event manager has already been created.");

        _bus = new EventBus(new EventBusOptions(32));
    }

    public void Destroy()
    {
        if (_bus == null)
            return;

        _bus.Dispose();
        _bus = null;
    }

    public EventSubscriptionScope CreateScope()
    {
        return GetBus().CreateScope();
    }
    public IDisposable AddListener<TEvent>(Action<TEvent> listener) where TEvent : class, IEventMessage
    {
        return GetBus().AddListener(listener);
    }
    public void SendMessage<TEvent>(TEvent message) where TEvent : class, IEventMessage
    {
        GetBus().SendMessage(message);
    }

    private EventBus GetBus()
    {
        if (_bus == null)
            throw new InvalidOperationException("Event manager has not been created. Call Create before using the event bus.");
        return _bus;
    }
}
