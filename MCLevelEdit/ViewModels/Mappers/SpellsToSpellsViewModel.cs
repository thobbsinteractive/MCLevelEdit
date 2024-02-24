using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MCLevelEdit.Model.Domain;
using System;

namespace MCLevelEdit.ViewModels.Mappers;

public static class SpellsToSpellsViewModel
{
    public static SpellsViewModel ToSpellsViewModel(this Spells spells)
    {
        return new SpellsViewModel()
        {
            Fireball = new AbilitiesViewModel(spells.Fireball, 1, "Fireball", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/fireball.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/fireball-white.png")))),
            Possess = new AbilitiesViewModel(spells.Possess, 2, "Possess", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/possess.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/possess-white.png")))),
            Accelerate = new AbilitiesViewModel(spells.Accelerate, 3, "Accelerate", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/accelerate.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/accelerate-white.png")))),
            Castle = new AbilitiesViewModel(spells.Castle, 4, "Castle", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/castle.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/castle-white.png")))),
            Heal = new AbilitiesViewModel(spells.Heal, 5, "Heal", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/heal.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/heal-white.png")))),
            Rebound = new AbilitiesViewModel(spells.Rebound, 6, "Rebound", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/rebound.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/rebound-white.png")))),
            Shield = new AbilitiesViewModel(spells.Shield, 7, "Shield", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/shield.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/shield-white.png")))),
            Invisible = new AbilitiesViewModel(spells.Invisible, 8, "Invisible", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/invisible.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/invisible-white.png")))),
            Earthquake = new AbilitiesViewModel(spells.Earthquake, 9, "Earthquake", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/earthquake.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/earthquake-white.png")))),
            Crater = new AbilitiesViewModel(spells.Crater, 10, "Crater", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/crater.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/crater-white.png")))),
            Meteor = new AbilitiesViewModel(spells.Meteor, 11, "Meteor", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/meteor.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/meteor-white.png")))),
            Volcano = new AbilitiesViewModel(spells.Volcano, 12, "Volcano", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/volcano.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/volcano-white.png")))),
            LightningBolt = new AbilitiesViewModel(spells.LightningBolt, 13, "Lightning Bolt", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/lightningbolt.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/lightningbolt-white.png")))),
            LightningStorm = new AbilitiesViewModel(spells.LightningStorm, 14, "Lighnting Storm", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/lightningstorm.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/lightningstorm-white.png")))),
            UndeadArmy = new AbilitiesViewModel(spells.UndeadArmy, 15, "Undead Army", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/undeadarmy.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/undeadarmy-white.png")))),
            ManaMagnet = new AbilitiesViewModel(spells.ManaMagnet, 16, "ManaMagnet", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/manamagnet.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/manamagnet-white.png")))),
            StealMana = new AbilitiesViewModel(spells.StealMana, 17, "Steal Mana", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/stealmana.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/stealmana-white.png")))),
            BeyondSight = new AbilitiesViewModel(spells.BeyondSight, 18, "Beyond Sight", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/beyondsight.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/beyondsight-white.png")))),
            Duel = new AbilitiesViewModel(spells.Duel, 19, "Duel", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/duel.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/duel-white.png")))),
            Teleport = new AbilitiesViewModel(spells.Teleport, 20, "Teleport", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/teleport.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/teleport-white.png")))),
            WallofFire = new AbilitiesViewModel(spells.WallofFire, 21, "Wall of Fire", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/walloffire.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/walloffire-white.png")))),
            ReverseAcceleration = new AbilitiesViewModel(spells.ReverseAcceleration, 22, "Reverse Acceleration", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/reverse.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/reverse-white.png")))),
            GlobalDeath = new AbilitiesViewModel(spells.GlobalDeath, 23, "Global Death", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/globaldeath.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/globaldeath-white.png")))),
            RapidFireball = new AbilitiesViewModel(spells.RapidFireball, 24, "Rapid Fireball", new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/rapidfireball.png"))), new Bitmap(AssetLoader.Open(new Uri("avares://MCLevelEdit/Assets/Spells/rapidfireball-white.png"))))
        };
    }
}
