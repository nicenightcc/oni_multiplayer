using System;

namespace MultiplayerMod.Core.Events;

public interface IEventSubscription {
    void Cancel();
}

public class EventSubscription : IEventSubscription {

    private readonly EventDispatcher dispatcher;
    private readonly Delegate action;
    private readonly Type type;
    private EventSubscription? chainNext;

    public EventSubscription(EventDispatcher dispatcher, Delegate action, Type type) {
        this.dispatcher = dispatcher;
        this.action = action;
        this.type = type;
    }

    public EventSubscription Subscribe<T>(Action<T> action) where T : IDispatchableEvent {
        return Subscribe<T>((@event, _) => action(@event));
    }

    public EventSubscription Subscribe<T>(Action<T, EventSubscription> action) where T : IDispatchableEvent {
        chainNext = dispatcher.Subscribe(action);
        return chainNext;
    }

    public void Cancel() {
        dispatcher.Unsubscribe(type, action);
        chainNext?.Cancel();
    }

}


public class EventSubscription<T> : IEventSubscription where T : IDispatchableEvent {

    private readonly EventDispatcher<T> dispatcher;
    private readonly Delegate action;

    public EventSubscription(EventDispatcher<T> dispatcher, Delegate action) {
        this.dispatcher = dispatcher;
        this.action = action;
    }

    public void Cancel() => dispatcher.Unsubscribe(action);

}
