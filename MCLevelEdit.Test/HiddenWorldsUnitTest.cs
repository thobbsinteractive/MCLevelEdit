using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using MCLevelEdit.Model.Domain.MC1;
using Mc2Map = MCLevelEdit.Model.Domain.MC2.Mc2Map;
using MCLevelEdit.Model.Repository;

namespace MCLevelEdit.Test
{
    public class HiddenWorldsTests
    {
        private Map _previousMap = null!;

        [SetUp]
        public void RememberMap() => _previousMap = MapRepository.Map;

        [TearDown]
        public void RestoreMap() => MapRepository.Map = _previousMap;

        [Test]
        public void OnlyWallOfFireIsNamedDifferently()
        {
            foreach (var spell in Enum.GetValues<Spell>())
            {
                var expected = spell == Spell.FireWall ? "HomingMeteor" : spell.ToString();
                Assert.That(HiddenWorldsSpellType.GetHiddenWorldsName(spell), Is.EqualTo(expected), spell.ToString());
                Assert.That(EntityCatalog.GetMc1EntityTypes(true).Spells[(int)spell].Model.Name, Is.EqualTo(expected));
                Assert.That(EntityCatalog.GetMc1EntityTypes(false).Spells[(int)spell].Model.Name, Is.EqualTo(spell.ToString()));
            }
        }

        [Test]
        public void SpellListsFollowTheOpenLevelsGame()
        {
            MapRepository.Map = new Map();
            Assert.That(TypeId.Spell.GetEntityTypeFromTypeIdAndModelId((int)Spell.FireWall)!.Model.Name, Is.EqualTo("FireWall"));

            MapRepository.Map.SetHiddenWorlds(true);
            var spell = TypeId.Spell.GetEntityTypeFromTypeIdAndModelId((int)Spell.FireWall)!;
            Assert.That(spell.Model.Name, Is.EqualTo("HomingMeteor"));
            // The dropdown of spells offers Homing Meteor too
            Assert.That(spell.ModelTypes.Single(m => m.Id == (int)Spell.FireWall).Name, Is.EqualTo("HomingMeteor"));
        }

        [Test]
        public void MarkingALevelRenamesItsSpellJars()
        {
            var map = new Map();
            MapRepository.Map = map;
            map.Entities.Add(new Entity { Id = 1, Position = new Position(0, 0), EntityType = EntityCatalog.GetMc1EntityTypes(false).Spells[(int)Spell.FireWall] });
            map.Entities.Add(new Entity { Id = 2, Position = new Position(0, 0), EntityType = EntityCatalog.GetMc1EntityTypes(false).Spells[(int)Spell.Meteor] });

            map.SetHiddenWorlds(true);
            Assert.That(map.IsHiddenWorlds, Is.True);
            Assert.That(map.Entities.Select(e => e.EntityType.Model.Name), Is.EqualTo(new[] { "HomingMeteor", "Meteor" }));
            // Only the name changes: the jar holds the same spell ID, so the level file is unchanged
            Assert.That(map.Entities[0].EntityType.Model.Id, Is.EqualTo((int)Spell.FireWall));

            map.SetHiddenWorlds(false);
            Assert.That(map.Entities.Select(e => e.EntityType.Model.Name), Is.EqualTo(new[] { "FireWall", "Meteor" }));
        }

        [Test]
        public void MagicCarpet2LevelsAreNeverHiddenWorlds()
        {
            var map = new Mc2Map();
            map.SetHiddenWorlds(true);
            Assert.That(map.IsHiddenWorlds, Is.False);
        }
    }
}
