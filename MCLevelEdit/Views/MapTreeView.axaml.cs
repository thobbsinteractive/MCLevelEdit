using Avalonia.Controls;
using MCLevelEdit.ViewModels;
using System.Collections.Generic;

namespace MCLevelEdit.Views;

public partial class MapTreeView : UserControl
{
    public MapTreeViewModel? MapTreeViewModel { get { return (MapTreeViewModel?)this.DataContext; } }

    public MapTreeView()
    {
        InitializeComponent();

        this.cboEntityType.SelectionChanged += CboEntityType_SelectionChanged;
    }

    private void CboEntityType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        int index = ((KeyValuePair<int, string>)this?.cboEntityType?.SelectedItem).Key;
        MapTreeViewModel?.OnCboEntityTypeSelectionChanged(index);
    }
}