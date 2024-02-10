using Avalonia.Controls;

namespace MCLevelEdit.Views;

public partial class ValidationWindow : Window
{
    public static ValidationWindow I;
    public ValidationWindow()
    {
        I = this;
        InitializeComponent();
    }
}