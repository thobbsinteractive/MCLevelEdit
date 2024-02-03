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

        this.WhenActivated(action => action(ViewModel!.ShowEntitiesDialog.RegisterHandler(DoShowEditEntitiesDialogAsync)));
        this.WhenActivated(action => action(ViewModel!.ShowGameSettingsDialog.RegisterHandler(DoShowGameDialogAsync)));
        this.WhenActivated(action => action(ViewModel!.ShowValidationResultsDialog.RegisterHandler(DoShowValidationResultsDialogAsync)));
    }

    private async Task DoShowEditEntitiesDialogAsync(InteractionContext<EntitiesTableViewModel,
                                    EntitiesTableViewModel?> interaction)
    {
        var dialog = new EntitiesWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<EntitiesTableViewModel?>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowGameDialogAsync(InteractionContext<EditGameSettingsViewModel,
                                EditGameSettingsViewModel?> interaction)
    {
        var dialog = new GameSettingsWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<EditGameSettingsViewModel?>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowValidationResultsDialogAsync(InteractionContext<ValidationResultsTableViewModel,
                            ValidationResultsTableViewModel?> interaction)
    {
        var dialog = new ValidationWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<ValidationResultsTableViewModel?>(this);
        interaction.SetOutput(result);
    }
}
