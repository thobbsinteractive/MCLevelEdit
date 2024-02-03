namespace MCLevelEdit.Model.Domain.Validation
{
    public static class Rules
    {
        public static ValidationResult HasPlayerSpawn(IList<Entity> Entities)
        {
            if (Entities is not null && Entities.Any() &&
                Entities.Where(e => e.EntityType.TypeId == TypeId.Spawn &&
                e.EntityType.Model.Id == (int)Spawn.Flyer1).DefaultIfEmpty().Count() > 0)
            {
                if (Entities.Where(e => e.EntityType.TypeId == TypeId.Spawn &&
                    e.EntityType.Model.Id == (int)Spawn.Flyer1).DefaultIfEmpty().Count() == 1)
                {
                    return new ValidationResult(0, Result.Fail, "Player Spawn is required!");
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
