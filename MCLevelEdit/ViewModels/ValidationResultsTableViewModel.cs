using Avalonia.Collections;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.Model.Enums;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class ValidationResultsTableViewModel : ReactiveObject
{
    private readonly IMapService _mapService;
    private Result _filter = Result.None;

    public IAvaloniaList<ValidationResultViewModel> ValidationResults { get; } = new AvaloniaList<ValidationResultViewModel>();
    public Result Filter
    {
        get => _filter;
        set
        {
            this.RaiseAndSetIfChanged(ref _filter, value);
        }
    }

    public ValidationResultsTableViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
    {
        _mapService = mapService;
        RefreshData();
    }

    private void RefreshData()
    {
        ValidationResults.Clear();
        var results = _mapService.GetValidationResults(Filter)?.Select(r => r.ToValidationResultViewModel());
        if (results is not null && results.Any())
        {
            ValidationResults.AddRange(results);
        }
    }
}
