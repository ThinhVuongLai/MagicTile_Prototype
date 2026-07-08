using System;
using System.Collections.Generic;

/// <summary>
/// Generic event bus — Subscribe, Unsubscribe, Publish với struct events.
/// Kế thừa Singleton&lt;T&gt; để lazy initialization và global access.
/// </summary>
public class EventBus : Singleton<EventBus>
{
    private readonly Dictionary<Type, Delegate> _handlers = new();

    private EventBus() { }

    /// <summary>
    /// Subscribe handler cho event type T (phải là struct).
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type t = typeof(T);
        if (_handlers.TryGetValue(t, out Delegate existing))
            _handlers[t] = Delegate.Combine(existing, handler);
        else
            _handlers[t] = handler;
    }

    /// <summary>
    /// Unsubscribe handler khỏi event type T.
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type t = typeof(T);
        if (_handlers.TryGetValue(t, out Delegate existing))
        {
            Delegate removed = Delegate.Remove(existing, handler);
            if (removed == null)
                _handlers.Remove(t);
            else
                _handlers[t] = removed;
        }
    }

    /// <summary>
    /// Publish event đến tất cả subscriber của type T.
    /// </summary>
    public void Publish<T>(T evt) where T : struct
    {
        Type t = typeof(T);
        if (_handlers.TryGetValue(t, out Delegate existing) && existing is Action<T> action)
            action(evt);
    }

    /// <summary>
    /// Xóa toàn bộ subscriptions.
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
    }

    protected override void OnDestroy()
    {
        Clear();
        base.OnDestroy();
    }
}
