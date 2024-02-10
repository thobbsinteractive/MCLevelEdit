namespace MCLevelEdit.Application.Model;

public delegate void PubSubEventHandler<T>(object sender, PubSubEventArgs<T> args);