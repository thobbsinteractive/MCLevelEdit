using MCLevelEdit.Application.Abstractions;

namespace MCLevelEdit.Application.Model;

public class EventAggregator<T> : IEventAggregator<T>
{
    private Dictionary<string, PubSubEventHandler<T>> events = new Dictionary<string, PubSubEventHandler<T>>();

    public void AddEvent(string name, PubSubEventHandler<T> handler)
    {
        if (!events.ContainsKey(name))
            events.Add(name, handler);
    }
    public void RaiseEvent(string name, object sender, PubSubEventArgs<T> args)
    {
        if (events.ContainsKey(name) && events[name] != null)
            events[name](sender, args);
    }
    public void RegisterEvent(string name, PubSubEventHandler<T> handler)
    {
        if (events.ContainsKey(name))
            events[name] += handler;
        else
            events.Add(name, handler);
    }
}
