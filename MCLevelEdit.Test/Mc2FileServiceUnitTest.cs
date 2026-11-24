using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using MCLevelEdit.Model.Domain.MC2;
using MCLevelEdit.Model.Repository;
using MCLevelEdit.ViewModels.Mappers;

namespace MCLevelEdit.Test
{
    public class Mc2FileServiceTests
    {
        // Set this to a folder of uncompressed MC2 levels (26116 bytes each) to round-trip every one of them
        private const string ALL_LEVELS_ENV = "MC2_LEVELS_DIR";

        private static string ResourcePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Resources", name);

        private static Mc2Map LoadMc2(string path) => (Mc2Map)new FileAdapter().LoadMap(path);

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00169b.dat")]
        public void LoadMc2MapFromFile(string name)
        {
            var map = LoadMc2(ResourcePath(name));

            Assert.That(new FileAdapter().LoadMap(ResourcePath(name)), Is.InstanceOf<Mc2Map>());
            Assert.That(map.GameVersion, Is.EqualTo(GameVersion.MagicCarpet2));
            Assert.That(map.Entities, Is.All.InstanceOf<Mc2Entity>());
            Assert.That(map.Wizards, Is.All.InstanceOf<Mc2Wizard>());
            Assert.That(map.MaxEntities, Is.EqualTo(LevelFormat.MC2_MAX_ENTITIES));
            Assert.That(map.Entities, Is.Not.Empty);
            Assert.That(map.Entities.Any(e => e.EntityType.TypeId == TypeId.Spawn), "Level should have a player spawn");
            Assert.That(map.Objectives, Has.Count.EqualTo(LevelFormat.MC2_OBJECTIVE_COUNT));
            Assert.That(map.Objectives.Any(o => o.Type != ObjectiveType.None), "Level should have at least one objective");
            Assert.That(map.StageVars, Has.Count.EqualTo(LevelFormat.MC2_STAGE_VAR_COUNT));
            Assert.That(map.Wizards[0].IsActive);
        }

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00169b.dat")]
        public void SaveMc2MapIsByteIdentical(string name)
        {
            AssertRoundTrip(ResourcePath(name));
        }

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00169b.dat")]
        public void SaveMc2MapWithoutSourceDataKeepsModelledFields(string name)
        {
            var original = Mc2LevelFormat.Read(File.ReadAllBytes(ResourcePath(name)));
            original.SourceData = null;

            var reloaded = Mc2LevelFormat.Read(Mc2LevelFormat.Write(original));

            Assert.That(reloaded.Entities.Select(Describe), Is.EqualTo(original.Entities.Select(Describe)));
            Assert.That(reloaded.Objectives.Select(o => (o.Type, o.Value, o.X, o.Y)),
                Is.EqualTo(original.Objectives.Select(o => (o.Type, o.Value, o.X, o.Y))));
            Assert.That(reloaded.StageVars.Select(s => (s.TypeAndFlags, s.NextStage, s.EntityIndex, s.TargetX, s.TargetY)),
                Is.EqualTo(original.StageVars.Select(s => (s.TypeAndFlags, s.NextStage, s.EntityIndex, s.TargetX, s.TargetY))));
            Assert.That(reloaded.Terrain.GenerationParameters.Seed, Is.EqualTo(original.Terrain.GenerationParameters.Seed));
            Assert.That(reloaded.Terrain.GenerationParameters.Raise, Is.EqualTo(original.Terrain.GenerationParameters.Raise));
            Assert.That(reloaded.Terrain.GenerationParameters.MapType, Is.EqualTo(original.Terrain.GenerationParameters.MapType));
            for (int w = 0; w < original.Wizards.Length; w++)
            {
                Assert.That(reloaded.Wizards[w].IsActive, Is.EqualTo(original.Wizards[w].IsActive));
                Assert.That(((Mc2Wizard)reloaded.Wizards[w]).StartingSpells, Is.EqualTo(((Mc2Wizard)original.Wizards[w]).StartingSpells));
                Assert.That(((Mc2Wizard)reloaded.Wizards[w]).BlockedSpells, Is.EqualTo(((Mc2Wizard)original.Wizards[w]).BlockedSpells));
                Assert.That(((Mc2Wizard)reloaded.Wizards[w]).Life, Is.EqualTo(((Mc2Wizard)original.Wizards[w]).Life));
            }
        }

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00169b.dat")]
        public void Mc2EntitiesSurviveTheEditorViewModel(string name)
        {
            var map = LoadMc2(ResourcePath(name));
            MapRepository.Map = map; // the editor resolves entity names against the open map's game

            foreach (var entity in map.Entities)
            {
                var afterEdit = entity.ToEntityViewModel().ToEntity();
                Assert.That(Describe(afterEdit), Is.EqualTo(Describe(entity)), $"Entity {entity.Id} changed");
            }
        }

        [TearDown]
        public void ResetOpenMap() => MapRepository.Map = new Map();

        [Test]
        public void Mc2EntitiesUseMc2Names()
        {
            var map = LoadMc2(ResourcePath("mc2_000008.dat"));

            Assert.That(TypeId.Creature.GetEntityTypeFromTypeIdAndModelId(20, GameVersion.MagicCarpet2)!.Model.Name, Is.EqualTo("Spider"));
            Assert.That(TypeId.Vase.GetEntityTypeFromTypeIdAndModelId(0, GameVersion.MagicCarpet2)!.Model.Name, Is.EqualTo("Fireball"));
            Assert.That(TypeId.Special.GetEntityTypeFromTypeIdAndModelId(3, GameVersion.MagicCarpet2)!.Model.Name, Is.EqualTo("ExitGate"));
            // MC1 names are unchanged
            Assert.That(TypeId.Creature.GetEntityTypeFromTypeIdAndModelId(1, GameVersion.MagicCarpet1)!.Model.Name, Is.EqualTo("Vulture"));

            Assert.That(map.Entities.Where(e => !e.IsInert()).All(e => e.EntityType.Game == GameVersion.MagicCarpet2));
            Assert.That(map.Entities.Where(e => e.EntityType.TypeId == TypeId.Creature).Select(e => e.EntityType.Model.Name),
                Has.None.StartsWith("Unknown"));
        }

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00169b.dat")]
        public void InertEntitiesAreHiddenButKept(string name)
        {
            var path = ResourcePath(name);
            var map = LoadMc2(path);

            Assert.That(map.Entities.Any(e => e.IsInert()), "Original MC2 levels contain type 0 leftovers");
            Assert.That(map.VisibleEntities.Any(e => e.IsInert()), Is.False);

            map.ValidateEntities();
            Assert.That(map.ValidationResults.Select(r => r.Message), Has.None.Contains("Fireball"),
                "MC1 spell-jar check should not run on MC2 levels");

            AssertRoundTrip(path);
        }

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00169b.dat")]
        public void Mc2LevelsHaveAPlayerExit(string name)
        {
            var map = LoadMc2(ResourcePath(name));

            map.ValidateEntities();

            Assert.That(map.ValidationResults.Where(r => r.Result != Model.Enums.Result.Pass).Select(r => r.Message),
                Has.None.Contains("Exit"));
            Assert.That(map.ValidationResults.Select(r => r.Message), Has.Some.EqualTo("HasPlayerExit"));
            Assert.That(map.ValidationResults.Select(r => r.Message), Has.Some.EqualTo("HasPlayerSpawn"));
        }

        [Test]
        public void Mc2LevelWithoutAnExitFails()
        {
            var map = LoadMc2(ResourcePath("mc2_00073f.dat"));
            var exit = map.Entities.Single(e => e.EntityType.TypeId == TypeId.Switch && e.EntityType.Model.Id == (int)Switch.ExitLevel);
            map.Entities.Remove(exit);

            map.ValidateEntities();

            Assert.That(map.ValidationResults.Where(r => r.Result == Model.Enums.Result.Fail).Select(r => r.Message),
                Has.Some.Contains("Player Exit"));
            Assert.That(map.ValidationResults.Where(r => r.Result == Model.Enums.Result.Warning).Select(r => r.Message),
                Has.Some.Contains("ExitGate has no ExitLevel switch"));
        }

        [Test]
        public void Mc2LevelsDoNotRunMc1Rules()
        {
            var map = LoadMc2(ResourcePath("mc2_00073f.dat"));

            map.ValidateEntities();

            Assert.That(map.ValidationResults.Select(r => r.Message), Has.None.Contains(nameof(Model.Domain.Validation.MC1.Mc1EntityRules.HasUniqueCoordinates)));
        }

        [Test]
        public void Mc2WizardSpellsSurviveTheEditorAndApplyEdits()
        {
            var map = LoadMc2(ResourcePath("mc2_00169b.dat"));
            MapRepository.Map = map;
            var wizard = (Mc2Wizard)map.Wizards[0];
            var before = (wizard.StartingSpells.ToArray(), wizard.SpellLevels.ToArray(), wizard.BlockedSpells.ToArray(), wizard.Life);

            var spells = wizard.ToMc2SpellViewModels();
            Assert.That(spells, Has.Count.EqualTo(LevelFormat.MC2_SPELL_COUNT));
            Assert.That(spells[0].Name, Is.EqualTo("Fireball"));

            var unchanged = new Mc2Wizard();
            unchanged.ApplyMc2Spells(spells);
            unchanged.Life = wizard.Life;
            Assert.That((unchanged.StartingSpells, unchanged.SpellLevels, unchanged.BlockedSpells, unchanged.Life), Is.EqualTo(before));

            // Toggle one spell and check only that byte changes, then that it is saved
            var spell = spells[4];
            spell.StartsWith = !spell.StartsWith;
            // A valid level (0 to 2) that differs from the original
            byte newLevel = (byte)((Math.Min(before.Item2[4], (byte)2) + 1) % 3);
            spell.Level = newLevel;
            spell.Blocked = !spell.Blocked;
            wizard.ApplyMc2Spells(spells);

            var reloaded = (Mc2Wizard)Mc2LevelFormat.Read(Mc2LevelFormat.Write(map)).Wizards[0];
            for (int i = 0; i < LevelFormat.MC2_SPELL_COUNT; i++)
            {
                if (i == 4) continue;
                Assert.That(reloaded.StartingSpells[i], Is.EqualTo(before.Item1[i]));
                Assert.That(reloaded.SpellLevels[i], Is.EqualTo(before.Item2[i]));
                Assert.That(reloaded.BlockedSpells[i], Is.EqualTo(before.Item3[i]));
            }
            Assert.That(reloaded.StartingSpells[4], Is.EqualTo(before.Item1[4] == 0 ? 1 : 0));
            Assert.That(reloaded.SpellLevels[4], Is.EqualTo(newLevel));
            Assert.That(reloaded.BlockedSpells[4], Is.EqualTo(before.Item3[4] == 0 ? 1 : 0));
        }

        [Test]
        public void AllMc2LevelsRoundTrip()
        {
            var folder = Environment.GetEnvironmentVariable(ALL_LEVELS_ENV);
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                Assert.Ignore($"Set {ALL_LEVELS_ENV} to a folder of uncompressed MC2 levels to run this test.");

            var files = Directory.GetFiles(folder!)
                .Where(f => new FileInfo(f).Length == LevelFormat.MC2_FILE_SIZE)
                .ToArray();
            Assert.That(files, Is.Not.Empty);

            foreach (var file in files)
            {
                AssertRoundTrip(file);

                var map = LoadMc2(file);
                MapRepository.Map = map;
                foreach (var entity in map.Entities)
                    Assert.That(Describe(entity.ToEntityViewModel().ToEntity()), Is.EqualTo(Describe(entity)), $"{file}: entity {entity.Id} changed");
            }
        }

        private static void AssertRoundTrip(string path)
        {
            var adapter = new FileAdapter();
            var testPath = Path.Combine(Directory.GetCurrentDirectory(), "mc2_test.dat");
            var before = File.ReadAllBytes(path);

            var map = adapter.LoadMap(path);
            Assert.That(adapter.SaveMap(map, testPath));
            var after = File.ReadAllBytes(testPath);

            Assert.That(after, Has.Length.EqualTo(before.Length));
            var firstDiff = Enumerable.Range(0, before.Length).FirstOrDefault(i => before[i] != after[i], -1);
            Assert.That(firstDiff, Is.EqualTo(-1), $"{Path.GetFileName(path)} differs at byte {firstDiff}");
        }

        private static string Describe(Entity e) =>
            $"{e.Id}:{(int)e.EntityType.TypeId}/{e.EntityType.Model.Id}@{e.Position.X},{e.Position.Y} " +
            $"{e.DisId} {e.SwitchSize} {e.SwitchId} {e.Parent} {e.Child} {(e as Mc2Entity)?.Par3}";
    }
}
