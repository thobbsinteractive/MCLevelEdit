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

    public SpellsViewModel Spells
    {
        get => _wizard.Spells;
    }

    public EditWizardViewModel(EventAggregator<object> eventAggregator, IMapService mapService, string wizardName)
    {
        _eventAggregator = eventAggregator;
        _mapService = mapService;
        var map = _mapService.GetMap();
        _wizard = map.Wizards.Where(w => w.Name.Equals(wizardName)).Select(w => w.ToWizardViewModel()).First();

        _wizard.Spells.Fireball.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Possess.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Accelerate.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Castle.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Heal.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Rebound.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Shield.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Invisible.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Earthquake.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Crater.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Meteor.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Volcano.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.LightningBolt.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.LightningStorm.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.UndeadArmy.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.ManaMagnet.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.StealMana.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.BeyondSight.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Duel.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.Teleport.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.WallofFire.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.ReverseAcceleration.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.GlobalDeath.SpellsUpdatedEvent += SpellsUpdatedEvent;
        _wizard.Spells.RapidFireball.SpellsUpdatedEvent += SpellsUpdatedEvent;
    }

    private void SpellsUpdatedEvent(object? sender, System.EventArgs e)
    {
        Update();
    }

    public void Update()
    {
        _mapService.UpdateWizard(_wizard.ToWizard());
        _eventAggregator.RaiseEvent("UpdateWizard", this, new PubSubEventArgs<object>(_wizard.ToWizard()));
    }
}
