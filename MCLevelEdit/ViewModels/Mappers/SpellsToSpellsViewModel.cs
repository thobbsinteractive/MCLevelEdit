using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class SpellsToSpellsViewModel
{
    public static SpellsViewModel ToSpellsViewModel(this Spells spells)
    {
        return new SpellsViewModel()
        {
            Fireball = new AbilitiesViewModel(spells.Fireball),
            Possess = new AbilitiesViewModel(spells.Possess),
            Accelerate = new AbilitiesViewModel(spells.Accelerate),
            Castle = new AbilitiesViewModel(spells.Castle),
            Heal = new AbilitiesViewModel(spells.Heal),
            Rebound = new AbilitiesViewModel(spells.Rebound),
            Shield = new AbilitiesViewModel(spells.Shield),
            Invisible = new AbilitiesViewModel(spells.Invisible),
            Earthquake = new AbilitiesViewModel(spells.Earthquake),
            Crater = new AbilitiesViewModel(spells.Crater),
            Meteor = new AbilitiesViewModel(spells.Meteor),
            Volcano = new AbilitiesViewModel(spells.Volcano),
            LightningBolt = new AbilitiesViewModel(spells.LightningBolt),
            LightningStorm = new AbilitiesViewModel(spells.LightningStorm),
            UndeadArmy = new AbilitiesViewModel(spells.UndeadArmy),
            ManaMagnet = new AbilitiesViewModel(spells.ManaMagnet),
            StealMana = new AbilitiesViewModel(spells.StealMana),
            BeyondSight = new AbilitiesViewModel(spells.BeyondSight),
            Duel = new AbilitiesViewModel(spells.Duel),
            Teleport = new AbilitiesViewModel(spells.Teleport),
            WallofFire = new AbilitiesViewModel(spells.WallofFire),
            ReverseAcceleration = new AbilitiesViewModel(spells.ReverseAcceleration),
            GlobalDeath = new AbilitiesViewModel(spells.GlobalDeath),
            RapidFireball = new AbilitiesViewModel(spells.RapidFireball)
        };
    }
}
