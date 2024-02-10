namespace MCLevelEdit.Application.Model;
public class PubSubEventArgs<T> : EventArgs
{
    public T Item { get; set; }

    public PubSubEventArgs(T item)
    {
        Item = item;
    }
}
