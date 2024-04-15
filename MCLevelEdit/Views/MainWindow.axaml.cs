using Avalonia.Input;
using Avalonia.ReactiveUI;
using MCLevelEdit.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MCLevelEdit.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public static MainWindow I;

    private bool _userClosed = false;

    public MainViewModel MainViewModel { get { return (MainViewModel)this.DataContext; } }

    public MainWindow()
    {
        I = this;
        InitializeComponent();

        this.WhenActivated(action => action(ViewModel!.ShowEntitiesDialog.RegisterHandler(DoShowEditEntitiesDialogAsync)));
        this.WhenActivated(action => action(ViewModel!.ShowSelectEntitiesDialog.RegisterHandler(DoShowSelectEntitiesDialogAsync)));
        this.WhenActivated(action => action(ViewModel!.ShowGameSettingsDialog.RegisterHandler(DoShowGameDialogAsync)));
        this.WhenActivated(action => action(ViewModel!.ShowValidationResultsDialog.RegisterHandler(DoShowValidationResultsDialogAsync)));
        this.WhenActivated(action => action(ViewModel!.ShowAboutDialog.RegisterHandler(DoShowAboutDialogAsync)));

        this.KeyDown += OnKeyDown;

        this.Closing += OnClosing;
    }

    private async void OnClosing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        if (!_userClosed)
        {
            e.Cancel = true;
            if (await MainViewModel.PromptSaveAndOrContinue())
            {
                _userClosed = true;
                this.Close();
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var keys = new Key[] { Key.Delete, Key.Up, Key.Down, Key.Left, Key.Right, Key.F1, Key.F5 };

        if (MainViewModel != null && keys.Contains(e.Key))
        {
            MainViewModel.OnKeyPressed(e.Key);
        }
    }

    private async Task DoShowEditEntitiesDialogAsync(InteractionContext<EntitiesTableViewModel,
                                    EntitiesTableViewModel?> interaction)
    {
        var dialog = new EntitiesWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<EntitiesTableViewModel?>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowSelectEntitiesDialogAsync(InteractionContext<IList<EntityViewModel>, IList<EntityViewModel>?> interaction)
    {
        var dialog = new SelectEntitiesWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<IList<EntityViewModel>?>(this);
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

    private async Task DoShowAboutDialogAsync(InteractionContext<AboutWindowViewModel, AboutWindowViewModel?> interaction)
    {
        var dialog = new AboutWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<AboutWindowViewModel?>(this);
        interaction.SetOutput(result);
    }
}
