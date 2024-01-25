using Avalonia.ReactiveUI;
using MCLevelEdit.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;

namespace MCLevelEdit.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public static MainWindow I;
    public MainWindow()
    {
        I = this;
        InitializeComponent();

        this.WhenActivated(action => action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(InteractionContext<EntitiesTableViewModel,
                                    EntitiesTableViewModel?> interaction)
    {
        var dialog = new EntitiesWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<EntitiesTableViewModel?>(this);
        interaction.SetOutput(result);
    }
}
