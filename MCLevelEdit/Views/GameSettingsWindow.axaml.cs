using Avalonia.Controls;

namespace MCLevelEdit.Views;

public partial class GameSettingsWindow : Window
{
    public static GameSettingsWindow I;
    public GameSettingsWindow()
    {
        I = this;
        InitializeComponent();
    }
}