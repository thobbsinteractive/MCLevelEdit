using Avalonia.Controls;
using Avalonia.Interactivity;
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
        this.btnEdit.Click += BtnEdit_Click;
    }

    private void BtnEdit_Click(object? sender, RoutedEventArgs e)
    {
        (this.Parent?.DataContext as MainViewModel)?.OnEditButtonClickedAsync();
    }

    private void CboEntityType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        int index = ((KeyValuePair<int, string>)this?.cboEntityType?.SelectedItem).Key;
        MapTreeViewModel?.OnCboEntityTypeSelectionChanged(index);
    }
}