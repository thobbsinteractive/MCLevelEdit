using ReactiveUI;

namespace MCLevelEdit.ViewModels;

public class SpellsViewModel : ReactiveObject
{
    public AbilitiesViewModel Fireball { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Possess { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Accelerate { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Castle { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Heal { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Rebound { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Shield { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Invisible { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Earthquake { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Crater { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Meteor { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Volcano { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel LightningBolt { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel LightningStorm { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel UndeadArmy { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel ManaMagnet { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel StealMana { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel BeyondSight { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Duel { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel Teleport { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel WallofFire { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel ReverseAcceleration { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel GlobalDeath { get; set; } = new AbilitiesViewModel();
    public AbilitiesViewModel RapidFireball { get; set; } = new AbilitiesViewModel();
}
