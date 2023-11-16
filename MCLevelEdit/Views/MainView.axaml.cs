using Avalonia.Controls;

namespace MCLevelEdit.Views;

public partial class MainView : UserControl
{
    public static MainView I;

    public MainView()
    {
        I = this;
        InitializeComponent();
    }
}
