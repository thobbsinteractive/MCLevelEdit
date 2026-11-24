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

        private static bool Is(Entity entity, TypeId type, int model) =>
            entity.EntityType.TypeId == type && entity.EntityType.Model.Id == model;
    }
}
