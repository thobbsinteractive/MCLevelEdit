using Avalonia.Controls;
using Avalonia.Media;
using MCLevelEdit.ViewModels;
using System.Windows.Input;

namespace MCLevelEdit.Views;

public partial class EditSpell : UserControl
{
    public AbilitiesViewModel? AbilitiesViewModel { get { return (AbilitiesViewModel?)this.DataContext; } }

    public string SpellName { get; set; } = string.Empty;
    public string SpellNumber { get; set; } = string.Empty;
    public ICommand Command { get; set; }

    public EditSpell()
    {
        InitializeComponent();

        this.DataContextChanged += EditSpell_DataContextChanged;
    }

    private void EditSpell_DataContextChanged(object? sender, System.EventArgs e)
    {
        RefreshData();
    }

    public void RefreshData()
    {
        if (AbilitiesViewModel != null)
        {
            btnSpell.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            btnSpell.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            btnSpell.Content = string.Empty;
            ToolTip.SetTip(btnSpell, $"{SpellName}: Cannot have");
            lblCross.IsVisible = false;

            if (AbilitiesViewModel.StartsWith)
            {
                btnSpell.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                ToolTip.SetTip(btnSpell, $"{SpellName}: Starts With");
                btnSpell.Content = SpellNumber;
            }
            else if (AbilitiesViewModel.WillLearnIfYouDo)
            {
                btnSpell.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                ToolTip.SetTip(btnSpell, $"{SpellName}: Will learn if you do");
                btnSpell.Content = SpellNumber;
            }
            else if (AbilitiesViewModel.CarriesCannotUse)
            {
                btnSpell.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                ToolTip.SetTip(btnSpell, $"{SpellName}: Carries, but cannot use");
                btnSpell.Content = SpellNumber;
                lblCross.IsVisible = true;
            }
        }
    }
}