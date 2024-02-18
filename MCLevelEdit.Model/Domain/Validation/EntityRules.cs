using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain.Validation
{
    public static class EntityRules
    {
        public static ValidationResult HasSwitch(Entity entity, IList<Entity> entities)
        {

            if (entity is not null && entities is not null && entities.Any())
            {
                if (entity.SwitchId > 0 && entity.SwitchId != ushort.MaxValue && entity.EntityType.TypeId != TypeId.Switch) 
                {
                    var sw = entities.Where(e => e.EntityType.TypeId == TypeId.Switch && e.SwitchId == entity.SwitchId).FirstOrDefault();

                    if (sw is null)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"{entity} has no Switch but has SwitchId {entity.SwitchId} set!");
                    } 
                    else if (entity.SwitchId != entity.DisId)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"{entity} should be the same as DisId {entity.DisId}!");
                    }
                    else
                    {
                        return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(HasSwitch)}");
                    }
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(HasSwitch)}");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"{entity} or Entities was null!");
            }
        }

        public static ValidationResult BuildingHasSwidAndDisIdAndParent(Entity entity)
        {

            if (entity is not null)
            {
                if (entity.EntityType.TypeId == TypeId.Effect && entity.EntityType.Model.Id == (int)Effect.VillagerBuilding)
                {
                    if (entity.DisId != ushort.MaxValue)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"{entity} Building should have DisId {ushort.MaxValue} set!");
                    }
                    if (entity.SwitchId != ushort.MaxValue)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"{entity} Building should have SwitchId {ushort.MaxValue} set!");
                    }
                    if (entity.Parent == 0)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"{entity} Building Parent {entity.Parent} should be greater than zero!");
                    }
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(BuildingHasSwidAndDisIdAndParent)}");
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(BuildingHasSwidAndDisIdAndParent)}");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"{entity} was null!");
            }
        }

        public static ValidationResult HasUniqueCoordinates(Entity entity, IList<Entity> entities)
        {

            if (entity is not null)
            {
                if (!entity.IsSwitch())
                {
                    var duplicateCoordEntities = entities.Where(e => e.Id != entity.Id && e.Position.Equals(entity.Position)).ToList();

                    if (duplicateCoordEntities?.Count > 0 && !duplicateCoordEntities.All(e => e.IsSwitch()))
                    {
                        return new ValidationResult(entity.Id, Result.Fail, $"{entity} has duplicate coordinates with another Entity {duplicateCoordEntities.FirstOrDefault().Id}");
                    }
                    else
                    {
                        return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(HasUniqueCoordinates)}");
                    }
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} is Switch");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"{entity} was null!");
            }
        }
    }
}
