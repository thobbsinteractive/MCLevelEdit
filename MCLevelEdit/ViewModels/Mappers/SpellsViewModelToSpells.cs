using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class SpellsViewModelSpells
{
    public static Spells ToSpells(this SpellsViewModel spellsViewModel)
    {
        return new Spells()
        {
            Fireball = spellsViewModel.Fireball.GetBytes(),
            Possess = spellsViewModel.Possess.GetBytes(),
            Accelerate = spellsViewModel.Accelerate.GetBytes(),
            Castle = spellsViewModel.Castle.GetBytes(),
            Heal = spellsViewModel.Heal.GetBytes(),
            Rebound = spellsViewModel.Rebound.GetBytes(),
            Shield = spellsViewModel.Shield.GetBytes(),
            Invisible = spellsViewModel.Invisible.GetBytes(),
            Earthquake = spellsViewModel.Earthquake.GetBytes(),
            Crater = spellsViewModel.Crater.GetBytes(),
            Meteor = spellsViewModel.Meteor.GetBytes(),
            Volcano = spellsViewModel.Volcano.GetBytes(),
            LightningBolt = spellsViewModel.LightningBolt.GetBytes(),
            LightningStorm = spellsViewModel.LightningStorm.GetBytes(),
            UndeadArmy = spellsViewModel.UndeadArmy.GetBytes(),
            ManaMagnet = spellsViewModel.ManaMagnet.GetBytes(),
            StealMana = spellsViewModel.StealMana.GetBytes(),
            BeyondSight = spellsViewModel.BeyondSight.GetBytes(),
            Duel = spellsViewModel.Duel.GetBytes(),
            Teleport = spellsViewModel.Teleport.GetBytes(),
            WallofFire = spellsViewModel.WallofFire.GetBytes(),
            ReverseAcceleration = spellsViewModel.ReverseAcceleration.GetBytes(),
            GlobalDeath = spellsViewModel.GlobalDeath.GetBytes(),
            RapidFireball = spellsViewModel.RapidFireball.GetBytes()
        };
    }
}
