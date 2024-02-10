﻿using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain.Validation
{
    public static class EntityRules
    {
        public static ValidationResult HasSwitch(Entity entity, IList<Entity> entities)
        {

            if (entity is not null && entities is not null && entities.Any())
            {
                if (entity.SwitchId > 0 && entity.EntityType.TypeId != TypeId.Switch) 
                {
                    var sw = entities.Where(e => e.EntityType.TypeId == TypeId.Switch && e.SwitchId == entity.SwitchId).FirstOrDefault();

                    if (sw is null)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"Entity has no Switch but has SwitchId {entity.SwitchId} set!");
                    } 
                    else if (entity.SwitchId != entity.DisId)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"SwitchId {entity.SwitchId} should be the same as DisId {entity.DisId}!");
                    }
                    else
                    {
                        return new ValidationResult(entity.Id, Result.Pass, nameof(HasSwitch));
                    }
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, nameof(HasSwitch));
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"Entity or Entities was null!");
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
                        return new ValidationResult(entity.Id, Result.Warning, $"Building should have DisId {ushort.MaxValue} set!");
                    }
                    if (entity.SwitchId != ushort.MaxValue)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"Building should have SwitchId {ushort.MaxValue} set!");
                    }
                    if (entity.Parent == 0)
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"Building Parent {entity.Parent} should be greater than zero!");
                    }
                    return new ValidationResult(entity.Id, Result.Pass, nameof(BuildingHasSwidAndDisIdAndParent));
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, nameof(BuildingHasSwidAndDisIdAndParent));
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"Entity was null!");
            }
        }
    }
}
