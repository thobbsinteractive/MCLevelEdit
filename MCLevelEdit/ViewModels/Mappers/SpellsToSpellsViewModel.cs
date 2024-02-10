using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class SpellsToSpellsViewModel
{
    public static SpellsViewModel ToSpellsViewModel(this Spells spells)
    {
        return new SpellsViewModel()
        {
            Fireball = new AbilitiesViewModel(spells.Fireball, 1, "Fireball"),
            Possess = new AbilitiesViewModel(spells.Possess, 2, "Possess"),
            Accelerate = new AbilitiesViewModel(spells.Accelerate, 3, "Accelerate"),
            Castle = new AbilitiesViewModel(spells.Castle, 4, "Castle"),
            Heal = new AbilitiesViewModel(spells.Heal, 5, "Heal"),
            Rebound = new AbilitiesViewModel(spells.Rebound, 6, "Rebound"),
            Shield = new AbilitiesViewModel(spells.Shield, 7, "Shield"),
            Invisible = new AbilitiesViewModel(spells.Invisible, 8, "Invisible"),
            Earthquake = new AbilitiesViewModel(spells.Earthquake, 9, "Earthquake"),
            Crater = new AbilitiesViewModel(spells.Crater, 10, "Crater"),
            Meteor = new AbilitiesViewModel(spells.Meteor, 11, "Meteor"),
            Volcano = new AbilitiesViewModel(spells.Volcano, 12, "Volcano"),
            LightningBolt = new AbilitiesViewModel(spells.LightningBolt, 13, "Lightning Bolt"),
            LightningStorm = new AbilitiesViewModel(spells.LightningStorm, 14, "Lighnting Storm"),
            UndeadArmy = new AbilitiesViewModel(spells.UndeadArmy, 15, "Undead Army"),
            ManaMagnet = new AbilitiesViewModel(spells.ManaMagnet, 16, "ManaMagnet"),
            StealMana = new AbilitiesViewModel(spells.StealMana, 17, "Steal Mana"),
            BeyondSight = new AbilitiesViewModel(spells.BeyondSight, 18, "Beyond Sight"),
            Duel = new AbilitiesViewModel(spells.Duel, 19, "Duel"),
            Teleport = new AbilitiesViewModel(spells.Teleport, 20, "Teleport"),
            WallofFire = new AbilitiesViewModel(spells.WallofFire, 21, "Wall of Fire"),
            ReverseAcceleration = new AbilitiesViewModel(spells.ReverseAcceleration, 22, "Reverse Acceleration"),
            GlobalDeath = new AbilitiesViewModel(spells.GlobalDeath, 23, "Global Death"),
            RapidFireball = new AbilitiesViewModel(spells.RapidFireball, 24, "Rapid Fireball")
        };
    }
}
