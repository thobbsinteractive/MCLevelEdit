using ReactiveUI;

namespace MCLevelEdit.ViewModels;

public class SpellsViewModel : ReactiveObject
{
    public AbilitiesViewModel Fireball { get; set; }
    public AbilitiesViewModel Possess { get; set; }
    public AbilitiesViewModel Accelerate { get; set; }
    public AbilitiesViewModel Castle { get; set; }
    public AbilitiesViewModel Heal { get; set; }
    public AbilitiesViewModel Rebound { get; set; }
    public AbilitiesViewModel Shield { get; set; }
    public AbilitiesViewModel Invisible { get; set; }
    public AbilitiesViewModel Earthquake { get; set; }
    public AbilitiesViewModel Crater { get; set; }
    public AbilitiesViewModel Meteor { get; set; }
    public AbilitiesViewModel Volcano { get; set; }
    public AbilitiesViewModel LightningBolt { get; set; }
    public AbilitiesViewModel LightningStorm { get; set; }
    public AbilitiesViewModel UndeadArmy { get; set; }
    public AbilitiesViewModel ManaMagnet { get; set; }
    public AbilitiesViewModel StealMana { get; set; }
    public AbilitiesViewModel BeyondSight { get; set; }
    public AbilitiesViewModel Duel { get; set; }
    public AbilitiesViewModel Teleport { get; set; }
    public AbilitiesViewModel WallofFire { get; set; }
    public AbilitiesViewModel ReverseAcceleration { get; set; }
    public AbilitiesViewModel GlobalDeath { get; set; }
    public AbilitiesViewModel RapidFireball { get; set; }
}
