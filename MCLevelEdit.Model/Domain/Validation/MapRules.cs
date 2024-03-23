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

        public static IList<ValidationResult> HasBasicSpells(Map map)
        {
            var ValidationResults = new List<ValidationResult>();

            if (map is not null && map.Entities is not null)
            {
                var fireballSpell = map.Entities.Where(e => e.IsFireballSpell()).FirstOrDefault();
                var possessionSpell = map.Entities.Where(e => e.IsPossessionSpell()).FirstOrDefault();
                var castleSpell = map.Entities.Where(e => e.IsCastleSpell()).FirstOrDefault();

                if (fireballSpell is not null)
                    ValidationResults.Add(new ValidationResult(fireballSpell.Id, Result.Pass, "Fireball Spell is present"));
                else
                    ValidationResults.Add(new ValidationResult(0, Result.Fail, "Fireball Spell is required to complete the level"));

                if (possessionSpell is not null)
                    ValidationResults.Add(new ValidationResult(possessionSpell.Id, Result.Pass, "Possession Spell is present"));
                else
                    ValidationResults.Add(new ValidationResult(0, Result.Fail, "Possession Spell is required to complete the level"));

                if (castleSpell is not null)
                    ValidationResults.Add(new ValidationResult(castleSpell.Id, Result.Pass, "Castle Spell is present"));
                else
                    ValidationResults.Add(new ValidationResult(0, Result.Warning, "It is a good idea to have the Castle Spell to help complete the level"));
            }

            return ValidationResults;
        }

        public static IList<ValidationResult> HasValidWizardParamters(Map map)
        {
            var ValidationResults = new List<ValidationResult>();

            if (map is not null && map.Entities is not null)
            {
                ValidationResults.Add(HasValidParameters(map.Wizards[1], Spawn.Flyer2, map.Entities));
                ValidationResults.Add(HasValidParameters(map.Wizards[2], Spawn.Flyer3, map.Entities));
                ValidationResults.Add(HasValidParameters(map.Wizards[3], Spawn.Flyer4, map.Entities));
                ValidationResults.Add(HasValidParameters(map.Wizards[4], Spawn.Flyer5, map.Entities));
                ValidationResults.Add(HasValidParameters(map.Wizards[5], Spawn.Flyer6, map.Entities));
                ValidationResults.Add(HasValidParameters(map.Wizards[6], Spawn.Flyer7, map.Entities));
                ValidationResults.Add(HasValidParameters(map.Wizards[7], Spawn.Flyer8, map.Entities));
            }
            return ValidationResults;
        }

        private static ValidationResult HasValidParameters(Wizard wizard, Spawn spawn, IList<Entity> entities)
        {
            if (wizard.IsActive)
            {
                if (wizard.CastleLevel > 0)
                {
                    if (wizard.CastleLevel > 7)
                    {
                        return new ValidationResult(0, Result.Fail, $"{wizard.Name} Castle Level parameter is too high (greater than 7)! Strange things will happen!");
                    }
                    else
                    {
                        if (wizard.Spells.Castle != (1, 1))
                        {
                            return new ValidationResult(0, Result.Fail, $"{wizard.Name} Castle Level parameter is set but they do not start with a Castle Spell!");
                        }
                        else
                        {
                            return new ValidationResult(0, Result.Pass, $"{wizard.Name} has Castle spell");
                        }
                    }
                }
                else
                {
                    return new ValidationResult(0, Result.Pass, $"{wizard.Name} does not have a Castle Level");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Pass, $"{wizard.Name} Is not active");
            }
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
