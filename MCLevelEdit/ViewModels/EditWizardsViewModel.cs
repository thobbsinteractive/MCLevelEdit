using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using ReactiveUI;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class EditWizardsViewModel : ReactiveObject
{
    protected readonly IMapService _mapService;
    protected readonly EventAggregator<object> _eventAggregator;
    private byte _wizardCount;

    public byte WizardCount
    {
        get => _wizardCount;
        set
        {
            if (value < 1)
                value = 1;

            if (value > 8)
                value = 8;

            this.RaiseAndSetIfChanged(ref _wizardCount, value);
            _mapService.SetActiveWizards(_wizardCount);
            _eventAggregator.RaiseEvent("RefreshWizards", this, new PubSubEventArgs<object>("RefreshWizards"));
        }
    }

    public EditWizardsViewModel(EventAggregator<object> eventAggregator, IMapService mapService)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;

        RefreshData();
    }

    public void RefreshData()
    {
        var map = _mapService.GetMap();
        var wizardCount = map.Wizards.Where(w => w.IsActive).Count();
        WizardCount = (byte)wizardCount;
    }
}
