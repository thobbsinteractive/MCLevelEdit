using Avalonia.Controls;
using Avalonia.Media;
using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Views;

public partial class EditSpells : UserControl
{
    public SpellsViewModel? SpellsViewModel { get { return (SpellsViewModel?)this.DataContext; } }

    public EditSpells()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SpellsViewModel != null)
        {

        }
    }

    private void BindButton(Button button, AbilitiesViewModel abilitiesViewModel)
    {
        if (button != null && abilitiesViewModel != null)
        {
            button.FontWeight = FontWeight.Bold;
            button.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            button.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            button.Content = string.Empty;

            if (abilitiesViewModel.StartsWith)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                button.Content = button.Tag;
            } else if (abilitiesViewModel.WillLearnIfYouDo) {
                button.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                button.Content = button.Tag;
            } else if (abilitiesViewModel.CarriesCannotUse)
            {
                
                button.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                button.Content = button.Tag;
            }
        }
    }
}