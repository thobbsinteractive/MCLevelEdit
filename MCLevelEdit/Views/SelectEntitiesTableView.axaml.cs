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
            int index = ((KeyValuePair<int, string>)this?.cboEntityType?.SelectedItem).Key;
            VmEntitiesTable?.OnCboEntityTypeSelectionChanged(index);
        }
    }
}
