using Avalonia;
using MCLevelEdit.Application.Model;
using MCLevelEdit.Application.Services;
using MCLevelEdit.Infrastructure.Adapters;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.MC2;
using MCLevelEdit.Model.Domain.Validation.MC2;
using MCLevelEdit.Model.Enums;
using MCLevelEdit.Model.Repository;
using MCLevelEdit.ViewModels;

namespace MCLevelEdit.Test
{
    public class Mc2ObjectivesTests
    {
        private static string ResourcePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Resources", name);

        private static Mc2Map Load(string name) => Mc2LevelFormat.Read(File.ReadAllBytes(ResourcePath(name)));

        [TestCase("mc2_000008.dat")]
        [TestCase("mc2_00073f.dat")]
        [TestCase("mc2_00109e.dat")]
        [TestCase("mc2_00169b.dat")]
        [TestCase("mc2_0041e2.dat")]
        [TestCase("mc2_00590d.dat")]
        public void TheGamesOwnObjectivesPointAtEntitiesThatAreThere(string name)
        {
            var map = Load(name);

            var fails = Mc2MapRules.CheckObjectives(map).Where(r => r.Result == Result.Fail).ToList();

            Assert.That(fails.Select(r => r.Message), Is.Empty);
        }

        // Kill wizard's Value is the wizard's number from 1: these levels point at their last wizard (2 of 2, 5 of 5)
        [TestCase("mc2_00073f.dat", 2, "Wizard 2")]
        [TestCase("mc2_00590d.dat", 5, "Wizard 5")]
        public void TheGamesKillWizardObjectivesPointAtAWizardInTheLevel(string name, int number, string wizard)
        {
            var map = Load(name);
            var objective = map.Objectives.Single(o => o.Type == ObjectiveType.KillPlayer);

            Assert.That(objective.Value, Is.EqualTo(number));
            Assert.That(map.Wizards[objective.Value - 1].Name, Is.EqualTo(wizard));
            Assert.That(map.Wizards[objective.Value - 1].IsActive);
            Assert.That(Mc2MapRules.CheckObjectives(map).Where(r => r.Message.Contains("Kill wizard")), Is.Empty);
        }

        [Test]
        public void ADeletedEntityLeavesItsObjectiveFailing()
        {
            var map = Load("mc2_00590d.dat");
            var objective = map.Objectives.First(o => o.Type == ObjectiveType.TargetEntity1);
            map.DeleteEntity(map.GetEntity(objective.Value)!);

            var results = Mc2MapRules.CheckObjectives(map);

            Assert.That(results.Any(r => r.Result == Result.Fail && r.Message.Contains($"entity {objective.Value}")));
        }

        [Test]
        public void ObjectivesOfTheWrongKindOrOffTheMapAreReported()
        {
            var map = Load("mc2_00590d.dat");
            var creature = map.VisibleEntities.First(e => e.EntityType.TypeId == TypeId.Creature);
            var building = map.VisibleEntities.First(e => e.IsBuilding());

            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.DestroyBuilding, Value = (short)creature.Id }, 1).Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.DestroyBuilding, Value = (short)building.Id }, 1), Is.Empty);
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.KillCreatureType, Value = (short)building.Id }, 1).Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.ReleasePoint, X = 256, Y = 10 }, 1).Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.KillPlayer, Value = 9 }, 1).Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.KillPlayer, Value = 0 }, 1).Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.CollectMana, Value = 0 }, 1).Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.KillAllPlayers }, 1), Is.Empty);
        }

        [Test]
        public void ALevelWithNoObjectivesIsWarned()
        {
            var map = Load("mc2_0041e2.dat");
            foreach (var objective in map.Objectives)
                objective.Type = ObjectiveType.None;

            Assert.That(Mc2MapRules.CheckObjectives(map).Any(r => r.Message.Contains("no objectives")));
        }

        [Test]
        public void ASecondCollectManaObjectiveFails()
        {
            var map = Load("mc2_000008.dat"); // Collect mana 15% in slot 4
            Assert.That(Mc2MapRules.CheckCollectMana(map.Objectives), Is.Empty);

            map.Objectives[5] = new Objective { Type = ObjectiveType.CollectMana, Value = 50 };
            var results = Mc2MapRules.CheckCollectMana(map.Objectives);

            Assert.That(results.Single().Result, Is.EqualTo(Result.Fail));
            Assert.That(results.Single().Message, Does.StartWith("Objective 6 ").And.Contain("objective 4"));
            Assert.That(Mc2MapRules.CheckObjectives(map).Any(r => r.Message == results.Single().Message));
        }

        [TestCase((short)1, false)]
        [TestCase((short)100, false)]
        [TestCase((short)0, true)]
        [TestCase((short)101, true)]
        [TestCase((short)-5, true)]
        public void ACollectManaShareOutsideOneToAHundredFails(short share, bool fails)
        {
            var map = Load("mc2_000008.dat");

            var results = Mc2MapRules.CheckObjective(map, new Objective { Type = ObjectiveType.CollectMana, Value = share }, 1);

            Assert.That(results.Any(r => r.Result == Result.Fail), Is.EqualTo(fails));
        }

        [Test]
        public void AnObjectiveAfterAnEmptySlotFails()
        {
            var map = Load("mc2_00590d.dat");
            map.Objectives[1].Type = ObjectiveType.None;

            var gaps = Mc2MapRules.CheckObjectiveOrder(map.Objectives);

            // slots 3, 4 and 5 are used, after the empty slot 2
            Assert.That(gaps.Select(r => r.Result), Is.EqualTo(new[] { Result.Fail, Result.Fail, Result.Fail }));
            Assert.That(gaps[0].Message, Does.StartWith("Objective 3 ").And.Contain("empty slot 2"));
            Assert.That(Mc2MapRules.CheckObjectiveOrder(Load("mc2_00590d.dat").Objectives), Is.Empty);
        }

        [Test]
        public void EditedObjectivesAreSaved()
        {
            var map = Load("mc2_00590d.dat");
            map.Objectives[0] = new Objective { Type = ObjectiveType.ReleasePoint, X = 12, Y = 34 };
            map.Objectives[7] = new Objective { Type = ObjectiveType.CollectMana, Value = 55 };

            var reloaded = Mc2LevelFormat.Read(Mc2LevelFormat.Write(map));

            Assert.That(reloaded.Objectives.Select(o => (o.Type, o.Value, o.X, o.Y)),
                Is.EqualTo(map.Objectives.Select(o => (o.Type, o.Value, o.X, o.Y))));
        }

        [Test]
        public void TheEditorPicksSwapsAndPadsObjectives()
        {
            var map = Load("mc2_00590d.dat");
            while (map.Objectives.Count > 3)
                map.Objectives.RemoveAt(map.Objectives.Count - 1);
            MapRepository.Map = map;

            var events = new EventAggregator<object>();
            var markers = new List<IList<(int Slot, Point Tile)>>();
            events.RegisterEvent("ShowObjectiveMarkers", (s, a) => markers.Add((IList<(int Slot, Point Tile)>)a.Item));
            var editor = new EditObjectivesViewModel(events, new MapService(events, null!, null!));

            editor.Show();
            Assert.That(map.Objectives, Has.Count.EqualTo(LevelFormat.MC2_OBJECTIVE_COUNT), "empty slots added up to 8");
            Assert.That(editor.Slots, Has.Count.EqualTo(LevelFormat.MC2_OBJECTIVE_COUNT));
            Assert.That(markers.Last(), Is.Not.Empty, "the objectives' entities are marked on the map");

            // slot 1 points at another entity, picked on the map
            var target = map.VisibleEntities.First(e => e.EntityType.TypeId == TypeId.Creature && e.Id != map.Objectives[0].Value);
            var changed = new List<string?>();
            editor.Slots[0].PropertyChanged += (s, e) => changed.Add(e.PropertyName);
            editor.Slots[0].PickEntityCommand.Execute().Subscribe();
            Assert.That(editor.PickSlot, Is.EqualTo(0));
            editor.OnMapClicked(new Point(target.Position.X, target.Position.Y), true, false);
            Assert.That(editor.PickSlot, Is.EqualTo(-1), "one pick, then the map is back to normal");
            var picked = map.GetEntity(map.Objectives[0].Value)!;
            Assert.That(picked.Position, Is.EqualTo(target.Position));
            Assert.That(editor.Slots[0].ShowEntity);
            Assert.That(editor.Slots[0].EntityText, Does.StartWith($"{picked.Id:D4}: "), "the entity picked is described");
            Assert.That(changed, Does.Contain(nameof(ObjectiveSlotViewModel.EntityText)));

            // the hidden wizard combo box leaves an entity objective alone (it set Value back to 0 after a pick), and
            // the wizard list is not made again when the wizards have not changed (a new list resets the combo box)
            Assert.That(changed, Does.Not.Contain(nameof(ObjectiveSlotViewModel.WizardNames)));
            editor.Slots[0].WizardIdx = 0;
            Assert.That(map.Objectives[0].Value, Is.EqualTo((short)picked.Id));

            // slot 4 (empty) made a release point, its place picked on the map
            editor.Slots[3].TypeIdx = ObjectiveSlotViewModel.Types.IndexOf(ObjectiveType.ReleasePoint);
            editor.Slots[3].PickPositionCommand.Execute().Subscribe();
            editor.OnMapClicked(new Point(40, 50), true, false);
            Assert.That((map.Objectives[3].Type, map.Objectives[3].X, map.Objectives[3].Y), Is.EqualTo((ObjectiveType.ReleasePoint, (short)40, (short)50)));

            // a right click stops picking and changes nothing
            editor.Slots[3].PickPositionCommand.Execute().Subscribe();
            editor.OnMapClicked(new Point(1, 1), false, true);
            Assert.That((map.Objectives[3].X, map.Objectives[3].Y), Is.EqualTo(((short)40, (short)50)));

            // slots swapped
            var first = map.Objectives[0];
            var second = map.Objectives[1];
            editor.Slots[0].MoveDownCommand.Execute().Subscribe();
            Assert.That(map.Objectives[0], Is.SameAs(second));
            Assert.That(map.Objectives[1], Is.SameAs(first));

            // an entity that is not there cannot be gone to (the level check reports it)
            editor.Slots[0].TypeIdx = ObjectiveSlotViewModel.Types.IndexOf(ObjectiveType.TargetEntity1);
            editor.Slots[0].EntityId = 1199;
            Assert.That(editor.Slots[0].CanGoToEntity, Is.False);
        }
    }
}
