using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain.Validation
{
    /// <summary>Rules for both Magic Carpet and Magic Carpet 2 levels.</summary>
    public static class SharedMapRules
    {
        public static ValidationResult HasPlayerSpawn(IList<Entity> entities)
        {
            int spawns = entities?.Count(e => e.EntityType.TypeId == TypeId.Spawn && e.EntityType.Model.Id == (int)Spawn.Flyer1) ?? 0;

            if (spawns == 0)
                return new ValidationResult(0, Result.Fail, "Player Spawn is required!");
            if (spawns > 1)
                return new ValidationResult(0, Result.Fail, "Too many Player Spawns!");
            return new ValidationResult(0, Result.Pass, nameof(HasPlayerSpawn));
        }
    }
}
