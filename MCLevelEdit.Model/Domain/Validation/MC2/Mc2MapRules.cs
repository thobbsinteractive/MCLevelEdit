using MCLevelEdit.Model.Domain.MC2;
using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain.Validation.MC2
{
    public static class Mc2MapRules
    {
        /// <summary>
        /// The player's exit: an ExitLevel switch (12) and its ExitGate (Special 3). In the game (remc2
        /// ActivateSwitchChain_6F2B0) a wizard who flies into the switch ends the level, flying into the gate. The level
        /// keeps only one gate (the last one made), and the switch and gate appear together, on the same switch.
        /// A second exit, ExitLevel2 (31) with ExitGate2 (Special 4), works the same way.
        /// </summary>
        public static IList<ValidationResult> HasPlayerExit(IList<Entity> entities)
        {
            var results = new List<ValidationResult>();
            entities ??= new List<Entity>();

            var exits = entities.Where(e => Is(e, TypeId.Switch, (int)Switch.ExitLevel)).ToList();
            var gates = entities.Where(e => Is(e, TypeId.Special, (int)Special.ExitGate)).ToList();

            if (exits.Count == 0)
                results.Add(new ValidationResult(0, Result.Fail, "Player Exit (ExitLevel switch) is required to finish the level!"));
            else if (exits.Count > 1)
                results.Add(new ValidationResult(exits[1].Id, Result.Warning, $"Too many Player Exits ({exits.Count} ExitLevel switches)!"));
            else
                results.Add(new ValidationResult(exits[0].Id, Result.Pass, nameof(HasPlayerExit)));

            results.AddRange(CheckExitGate(exits, gates, "ExitLevel", "ExitGate"));

            var exits2 = entities.Where(e => Is(e, TypeId.Switch, (int)Switch.ExitLevel2)).ToList();
            var gates2 = entities.Where(e => Is(e, TypeId.Special, (int)Special.ExitGate2)).ToList();
            if (exits2.Count == 0 && gates2.Count > 0)
                results.Add(new ValidationResult(gates2[0].Id, Result.Warning, $"{gates2[0]} ExitGate2 has no ExitLevel2 switch to fly into it!"));
            else if (exits2.Count > 0)
                results.AddRange(CheckExitGate(exits2, gates2, "ExitLevel2", "ExitGate2"));

            return results;
        }

        private static IEnumerable<ValidationResult> CheckExitGate(IList<Entity> exits, IList<Entity> gates, string exitName, string gateName)
        {
            if (gates.Count == 0)
            {
                if (exits.Count > 0)
                    yield return new ValidationResult(exits[0].Id, Result.Warning, $"{exits[0]} {exitName} has no {gateName} for the player to fly into!");
                yield break;
            }
            if (gates.Count > 1)
                yield return new ValidationResult(gates[0].Id, Result.Warning, $"Too many {gateName}s ({gates.Count}): only the last one is used!");
            if (exits.Count == 0)
            {
                yield return new ValidationResult(gates[0].Id, Result.Warning, $"{gates[0]} {gateName} has no {exitName} switch to fly into it!");
                yield break;
            }

            var gate = gates[gates.Count - 1];
            foreach (var exit in exits.Where(e => e.DisId != gate.DisId))
                yield return new ValidationResult(exit.Id, Result.Warning,
                    $"{exit} {exitName} DisId {exit.DisId} should be the same as its {gateName}'s DisId {gate.DisId}, so they appear together!");
        }

        /// <summary>
        /// The level's objectives: each one points at an entity that is there (and of the right kind), a place on the
        /// map, a wizard or a share of the mana that makes sense. An entity deleted leaves its objective pointing at an
        /// empty slot, which the next entity added would take.
        /// </summary>
        public static IList<ValidationResult> CheckObjectives(Mc2Map map)
        {
            var results = new List<ValidationResult>();
            if (map?.Objectives is null)
                return results;

            for (int i = 0; i < map.Objectives.Count; i++)
                results.AddRange(CheckObjective(map, map.Objectives[i], i + 1));
            results.AddRange(CheckObjectiveOrder(map.Objectives));
            results.AddRange(CheckCollectMana(map.Objectives));

            if (map.Objectives.All(o => o.Type == ObjectiveType.None))
                results.Add(new ValidationResult(0, Result.Warning, "The level has no objectives!"));

            return results;
        }

        /// <summary>
        /// The objectives fill the slots from the first, with no empty slot before one that is used: every level of the
        /// game seen so far does this, and an empty slot may end the list for the game, so the objectives after it would
        /// never be given. Each used slot after an empty one fails.
        /// </summary>
        public static IList<ValidationResult> CheckObjectiveOrder(IList<Objective> objectives)
        {
            var results = new List<ValidationResult>();
            if (objectives is null)
                return results;

            int firstEmpty = -1;
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i] is null || objectives[i].Type == ObjectiveType.None)
                {
                    if (firstEmpty < 0)
                        firstEmpty = i;
                }
                else if (firstEmpty >= 0)
                {
                    results.Add(new ValidationResult(0, Result.Fail,
                        $"Objective {i + 1} comes after the empty slot {firstEmpty + 1}: the objectives have to fill the slots from the first, with no gaps!"));
                }
            }
            return results;
        }

        /// <summary>
        /// Collect mana: a level has one at most (no level of the game seen has two, and a second share of the same mana
        /// asks for nothing new). The share itself (1 to 100%) is checked with each objective.
        /// </summary>
        public static IList<ValidationResult> CheckCollectMana(IList<Objective> objectives)
        {
            var results = new List<ValidationResult>();
            var slots = (objectives ?? new List<Objective>())
                .Select((o, i) => (Objective: o, Number: i + 1))
                .Where(o => o.Objective?.Type == ObjectiveType.CollectMana)
                .ToList();
            foreach (var extra in slots.Skip(1))
                results.Add(new ValidationResult(0, Result.Fail,
                    $"Objective {extra.Number} (Collect mana) is a second Collect mana objective, after objective {slots[0].Number}: a level can have only one!"));
            return results;
        }

        /// <summary>One objective (its slot's number, from 1), checked against the level's entities and wizards.</summary>
        public static IList<ValidationResult> CheckObjective(Map map, Objective objective, int number)
        {
            var results = new List<ValidationResult>();
            if (objective is null || objective.Type == ObjectiveType.None)
                return results;

            if (!Enum.IsDefined(objective.Type))
            {
                results.Add(new ValidationResult(0, Result.Warning, $"Objective {number} has an unknown type {(int)objective.Type}!"));
                return results;
            }
            string name = $"Objective {number} ({ObjectiveTypeInfo.Name(objective.Type)})";

            if (ObjectiveTypeInfo.UsesEntity(objective.Type))
            {
                var entity = map.GetEntity(objective.Value);
                if (entity is null || entity.IsInert())
                    results.Add(new ValidationResult(0, Result.Fail, $"{name} points at entity {objective.Value}, which is not in the level!"));
                else if (objective.Type == ObjectiveType.DestroyBuilding && !entity.IsBuilding())
                    results.Add(new ValidationResult(entity.Id, Result.Fail, $"{name} points at {entity}, which is not a building!"));
                else if (objective.Type == ObjectiveType.KillCreatureType && entity.EntityType.TypeId != TypeId.Creature)
                    results.Add(new ValidationResult(entity.Id, Result.Fail, $"{name} points at {entity}, which is not a creature!"));
            }

            if (ObjectiveTypeInfo.UsesPosition(objective.Type)
                && (objective.X < 0 || objective.X >= Globals.MAX_MAP_SIZE || objective.Y < 0 || objective.Y >= Globals.MAX_MAP_SIZE))
                results.Add(new ValidationResult(0, Result.Fail, $"{name} is at ({objective.X},{objective.Y}), off the map (0 to 255)!"));

            if (ObjectiveTypeInfo.UsesWizard(objective.Type))
            {
                // Value is the wizard's number, from 1 (the player): 2 is Wizard 2, its slot 1 (the levels of the game seen
                // point at their last wizard this way, e.g. 5 in a level of 5 wizards)
                if (objective.Value < 1 || objective.Value > map.Wizards.Length)
                    results.Add(new ValidationResult(0, Result.Fail, $"{name} points at wizard {objective.Value}, but the wizards are numbered 1 to {map.Wizards.Length}!"));
                else if (!map.Wizards[objective.Value - 1].IsActive)
                    results.Add(new ValidationResult(0, Result.Warning, $"{name} points at {map.Wizards[objective.Value - 1].Name}, who is not in the level!"));
            }

            if (ObjectiveTypeInfo.UsesPercent(objective.Type) && (objective.Value < 1 || objective.Value > 100))
                results.Add(new ValidationResult(0, Result.Fail, $"{name} asks for {objective.Value}% of the mana: it should be 1 to 100!"));

            return results;
        }

        private static bool Is(Entity entity, TypeId type, int model) =>
            entity.EntityType.TypeId == type && entity.EntityType.Model.Id == model;
    }
}
