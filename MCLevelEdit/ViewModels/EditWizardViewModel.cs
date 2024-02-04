using MCLevelEdit.Application.Model;
using MCLevelEdit.Model.Abstractions;
using MCLevelEdit.ViewModels.Mappers;
using ReactiveUI;
using System.Linq;

namespace MCLevelEdit.ViewModels;

public class EditWizardViewModel : ReactiveObject
{
    protected readonly IMapService _mapService;
    protected readonly EventAggregator<object> _eventAggregator;

    private readonly WizardViewModel _wizard;

    public byte Agression
    {
        get => _wizard.Agression;
        set
        {
            _wizard.Agression = value;
            this.RaisePropertyChanged(nameof(Agression));
            Update();
        }
    }

    public byte Perception
    {
        get => _wizard.Perception;
        set
        {
            _wizard.Perception = value;
            this.RaisePropertyChanged(nameof(Perception));
            Update();
        }
    }

    public byte Reflexes
    {
        get => _wizard.Reflexes;
        set
        {
            _wizard.Reflexes = value;
            this.RaisePropertyChanged(nameof(Reflexes));
            Update();
        }
    }

    public byte CastleLevel
    {
        get => _wizard.CastleLevel;
        set
        {
            _wizard.CastleLevel = value;
            this.RaisePropertyChanged(nameof(CastleLevel));
            Update();
        }
    }

    public EditWizardViewModel(EventAggregator<object> eventAggregator, IMapService mapService, string wizardName)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        var map = _mapService.GetMap();
        _wizard = map.Wizards.Where(w => w.Name.Equals(wizardName)).Select(w => w.ToWizardViewModel()).First();
    }

    public void Update()
    {
        _mapService.UpdateWizard(_wizard.ToWizard());
    }
}
