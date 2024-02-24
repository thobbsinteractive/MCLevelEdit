using Avalonia;
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
    private readonly EventAggregator<object> _eventAggregator;
    private Result _filter = Result.None;

    public IAvaloniaList<ValidationResultViewModel> ValidationResults { get; } = new AvaloniaList<ValidationResultViewModel>();
    public Result Filter
    {
        get => _filter;
        set
        {
            this.RaiseAndSetIfChanged(ref _filter, value);
            RefreshData();
        }
    }

    public ValidationResultsTableViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
    }

    public void OnValidationResultsDoubleClicked(ValidationResultViewModel result)
    {
        var entity = _mapService.GetEntity((ushort)result.EntityId);

        if (entity != null) {
            (Point, bool, bool) cursorEvent = (new Point(entity.Position.X, entity.Position.Y), true, false);
            _eventAggregator.RaiseEvent("OnCursorClicked", this, new PubSubEventArgs<object>(cursorEvent));
        }
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
