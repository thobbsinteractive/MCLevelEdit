using Avalonia.Controls;

namespace MCLevelEdit.Views;

public partial class PackageLevelsWindow : Window
{
    public static PackageLevelsWindow I;
    public PackageLevelsWindow()
    {
        I = this;
        InitializeComponent();
    }
}