using Avalonia.Controls;
using MCLevelEdit.ViewModels;
using System.Collections.Generic;

namespace MCLevelEdit.Views
{
    public partial class SelectEntitiesTableView : UserControl
    {
        public EntitiesTableViewModel? VmEntitiesTable { get { return (EntitiesTableViewModel?)this.DataContext; } }

        public SelectEntitiesTableView()
        {
            InitializeComponent();

            this.dgEntities.SelectionChanged += DgEntities_SelectionChanged;
            this.cboEntityType.SelectionChanged += CboEntityType_SelectionChanged;
            this.Unloaded += EntitiesTableView_Unloaded; 
        }

        private void CboEntityType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            int index = ((KeyValuePair<int, string>)this?.cboEntityType?.SelectedItem).Key;
            VmEntitiesTable?.OnCboEntityTypeSelectionChanged(index);
        }

        private void EntitiesTableView_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            VmEntitiesTable?.OnUnload();
        }

        private void DgEntities_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectItems = this.dgEntities.SelectedItems;
            IList<EntityViewModel>? selectedEntities = new List<EntityViewModel>();

            foreach (var item in selectItems)
            {
                selectedEntities.Add((EntityViewModel)item);
            }

            VmEntitiesTable?.OnSelectedItemsChanged(this, selectedEntities);
        }
    }
}
