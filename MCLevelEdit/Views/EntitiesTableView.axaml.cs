using Avalonia.Controls;
using MCLevelEdit.ViewModels;
using System.Collections.Generic;

namespace MCLevelEdit.Views
{
    public partial class EntitiesTableView : UserControl
    {
        public EntitiesTableViewModel? VmEntitiesTable { get { return (EntitiesTableViewModel?)this.DataContext; } }

        public EntitiesTableView()
        {
            InitializeComponent();

            this.dgEntities.SelectionChanged += DgEntities_SelectionChanged;
        }

        private void DgEntities_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectItems = this.dgEntities.SelectedItems;
            IList<EntityViewModel>? selectedEntities = new List<EntityViewModel>();

            foreach (var item in selectItems)
            {
                selectedEntities.Add((EntityViewModel)item);
            }

            VmEntitiesTable.OnSelectedItemsChanged(this, selectedEntities);
        }
    }
}
