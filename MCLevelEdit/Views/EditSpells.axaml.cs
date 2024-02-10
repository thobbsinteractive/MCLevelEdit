using Avalonia.Controls;
using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Views;

public partial class EditSpells : UserControl
{
    public SpellsViewModel? SpellsViewModel { get { return (SpellsViewModel?)this.DataContext; } }

    public EditSpells()
    {
        InitializeComponent();
    }
}