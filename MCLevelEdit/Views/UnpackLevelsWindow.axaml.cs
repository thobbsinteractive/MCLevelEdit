using Avalonia.Controls;

namespace MCLevelEdit.Views;

public partial class UnpackLevelsWindow : Window
{
    public static UnpackLevelsWindow I;
    public UnpackLevelsWindow()
    {
        I = this;
        InitializeComponent();
    }
}