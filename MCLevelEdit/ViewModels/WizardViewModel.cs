using CommunityToolkit.Mvvm.ComponentModel;

namespace MCLevelEdit.ViewModels
{
    public partial class WizardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private byte _agression;

        [ObservableProperty]
        private byte _perception;

        [ObservableProperty]
        private byte _reflexes;

        [ObservableProperty]
        private byte _castleLevel;

        [ObservableProperty]
        private SpellsViewModel _spells = new SpellsViewModel();

        // Magic Carpet 2 only
        [ObservableProperty]
        private short _life;

        public System.Collections.Generic.List<Mc2SpellViewModel> Mc2Spells { get; set; } = new();
    }
}
