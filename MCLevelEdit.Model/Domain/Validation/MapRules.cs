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

        public static IList<ValidationResult> HasCorrectNumberOfWizardSpawns(Map map)
        {
            var ValidationResults = new List<ValidationResult>();

            if (map is not null && map.Entities is not null)
            {
                ValidationResults.Add(HasWizardSpawn(map.Wizards[1], Spawn.Flyer2, map.Entities));
                ValidationResults.Add(HasWizardSpawn(map.Wizards[2], Spawn.Flyer3, map.Entities));
                ValidationResults.Add(HasWizardSpawn(map.Wizards[3], Spawn.Flyer4, map.Entities));
                ValidationResults.Add(HasWizardSpawn(map.Wizards[4], Spawn.Flyer5, map.Entities));
                ValidationResults.Add(HasWizardSpawn(map.Wizards[5], Spawn.Flyer6, map.Entities));
                ValidationResults.Add(HasWizardSpawn(map.Wizards[6], Spawn.Flyer7, map.Entities));
                ValidationResults.Add(HasWizardSpawn(map.Wizards[7], Spawn.Flyer8, map.Entities));
            }
            return ValidationResults;
        }

        private static ValidationResult HasWizardSpawn(Wizard wizard, Spawn spawn, IList<Entity> entities)
        {
            if (wizard.IsActive)
            {
                var spawnCount = entities?.Where(e => e.EntityType.TypeId == TypeId.Spawn && e.EntityType.Model.Id == (int)spawn).Count();

                if (spawnCount > 0)
                {
                    if (spawnCount == 1)
                    {
                        return new ValidationResult(0, Result.Pass, nameof(HasWizardSpawn));
                    }
                    else
                    {
                        return new ValidationResult(0, Result.Fail, $"Too many {wizard.Name} Spawns ({spawn})!");
                    }
                }
                else
                {
                    return new ValidationResult(0, Result.Fail, $"{wizard.Name} Spawn ({spawn}) is required!");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Pass, $"{wizard.Name} Spawn ({spawn}) is not required");
            }
        }
    }
}
