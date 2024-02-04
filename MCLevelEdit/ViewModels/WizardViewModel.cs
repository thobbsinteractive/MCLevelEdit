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
    }
}
