using Avalonia.Controls;
using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Views
{
    public partial class EntityToolBarView : UserControl
    {
        private EntityToolBarViewModel EntityViewModel
        { 
            get
            {
                return (EntityToolBarViewModel)DataContext;
            } 
        }

        public EntityToolBarView()
        {
            InitializeComponent();
        }

        private void ToggleButtons(bool enable)
        {
            this.btnCursor.IsChecked = enable;
            this.btnCreatures.IsChecked = enable;
            this.btnScenery.IsChecked = enable;
            this.btnEffects.IsChecked = enable;
            this.btnSpells.IsChecked = enable;
            this.btnSwitches.IsChecked = enable;
            this.btnWeathers.IsChecked = enable;
            this.btnSpawns.IsChecked = enable;
            this.cboEntityModelType.SelectedIndex = 0;
        }

        private void btnCursorClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnCursor.IsChecked = true;
            EntityViewModel.ClearSelection();
        }

        private void btnCreatureClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnCreatures.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Creature);
            cboEntityModelType.SelectedIndex = 0;
        }

        private void btnSceneryClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnScenery.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Scenery);
            cboEntityModelType.SelectedIndex = 0;
        }

        private void btnEffectsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnEffects.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Effect);
            cboEntityModelType.SelectedIndex = 0;
        }

        private void btnSpellsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnSpells.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Spell);
            cboEntityModelType.SelectedIndex = 0;
        }

        private void btnSwitchesClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnSwitches.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Switch);
            cboEntityModelType.SelectedIndex = 0;
        }

        private void btnWeathersClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnWeathers.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Weather);
            cboEntityModelType.SelectedIndex = 0;
        }

        private void btnSpawnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ToggleButtons(false);
            this.btnSpawns.IsChecked = true;
            EntityViewModel.OnEntityTypeSelected(Model.Domain.TypeId.Spawn);
            cboEntityModelType.SelectedIndex = 0;
        }
    }
}
