using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace MCLevelEdit.Views;

public partial class MapTreeView : UserControl
{
    public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<MapTreeView, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    protected virtual void OnValueChanged()
    {
        RoutedEventArgs args = new RoutedEventArgs(ValueChangedEvent);
        RaiseEvent(args);
    }

    public MapTreeView()
    {
        InitializeComponent();
    }
}