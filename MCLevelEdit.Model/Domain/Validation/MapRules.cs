using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain.Validation
{
    public static class MapRules
    {
        public static ValidationResult HasPlayerSpawn(IList<Entity> entities)
        {
            if (entities is not null && entities.Any() &&
                entities.Where(e => e.EntityType.TypeId == TypeId.Spawn &&
                e.EntityType.Model.Id == (int)Spawn.Flyer1).Count() > 0)
            {
                if (entities.Where(e => e.EntityType.TypeId == TypeId.Spawn &&
                    e.EntityType.Model.Id == (int)Spawn.Flyer1).Count() == 1)
                {
                    return new ValidationResult(0, Result.Pass, nameof(HasPlayerSpawn));
                }
                else
                {
                    return new ValidationResult(0, Result.Fail, "Too many Player Spawns!");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, "Player Spawn is required!");
            }
        }
    }
}
