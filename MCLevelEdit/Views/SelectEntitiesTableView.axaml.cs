using Avalonia.Controls;
using MCLevelEdit.ViewModels;
using System.Collections.Generic;

namespace MCLevelEdit.Views
{
    public partial class SelectEntitiesTableView : UserControl
    {
        public SelectEntitiesTableViewModel? VmEntitiesTable { get { return (SelectEntitiesTableViewModel?)this.DataContext; } }

        public SelectEntitiesTableView()
        {
            InitializeComponent();

            this.cboEntityType.SelectionChanged += CboEntityType_SelectionChanged;
        }

        private void CboEntityType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // nothing chosen (the list being reset): nothing to filter by yet
            if (this.cboEntityType?.SelectedItem is not KeyValuePair<int, string> selected)
                return;
            int index = selected.Key;
            VmEntitiesTable?.OnCboEntityTypeSelectionChanged(index);
        }
    }
}
