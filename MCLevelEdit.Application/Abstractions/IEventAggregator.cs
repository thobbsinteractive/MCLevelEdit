using MCLevelEdit.Application.Model;

namespace MCLevelEdit.Application.Abstractions;

internal interface IEventAggregator<T>
{
    public void AddEvent(string name, PubSubEventHandler<T> handler);
    public void RaiseEvent(string name, object sender, PubSubEventArgs<T> args);
    public void RegisterEvent(string name, PubSubEventHandler<T> handler); 
}
