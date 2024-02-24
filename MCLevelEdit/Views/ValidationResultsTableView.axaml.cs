using Avalonia.Controls;
using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Views;

public partial class ValidationResultsTableView : UserControl
{
    public ValidationResultsTableViewModel ValidationResultsTableViewModel 
    {
        get 
        { 
            return (ValidationResultsTableViewModel)this.DataContext;
        }
    }

    public ValidationResultsTableView()
    {
        InitializeComponent();
    }

    private void DataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var validationResult = this.dgResults.SelectedItem;
        if (validationResult != null && ValidationResultsTableViewModel != null)
        {
            ValidationResultsTableViewModel.OnValidationResultsDoubleClicked((ValidationResultViewModel)validationResult);
            ValidationWindow.I.Close();
        }
    }
}