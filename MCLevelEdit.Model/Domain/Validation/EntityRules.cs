﻿using MCLevelEdit.Model.Enums;

namespace MCLevelEdit.Model.Domain.Validation
{
    public static class EntityRules
    {
        public static ValidationResult HasSwitch(Entity entity, IList<Entity> entities)
        {

            if (entity is not null && (entities?.Any() ?? false))
            {
                if (entity.SwitchId > 0 && entity.DisId != ushort.MaxValue && entity.SwitchId != ushort.MaxValue && entity.EntityType.TypeId != TypeId.Switch) 
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

        public static ValidationResult PathEntityCannotSameChildAndParent(Entity entity, int entityTypeId)
        {

            if (entity is not null)
            {
                if (entity.EntityType.TypeId == TypeId.Effect && entity.EntityType.Model.Id == entityTypeId)
                {
                    if (entity.Id == entity.Child)
                    {
                        return new ValidationResult(entity.Id, Result.Fail, $"{entity} CANNOT have same Id as its Child Id!");
                    }
                    if (entity.Id == entity.Parent)
                    {
                        return new ValidationResult(entity.Id, Result.Fail, $"{entity} CANNOT have same Id as its Parent Id!");
                    }
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(PathEntityCannotSameChildAndParent)}");
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(PathEntityCannotSameChildAndParent)}");
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
                if (!entity.IsSwitch() && !entity.IsCanyonOrRidge() && !entity.IsWall())
                {
                    var duplicateCoordEntities = entities.Where(e => e.Id != entity.Id && e.Position.Equals(entity.Position)).ToList();

                    if (duplicateCoordEntities?.Count > 0 && !duplicateCoordEntities.All(e => e.IsSwitch()))
                    {
                        return new ValidationResult(entity.Id, Result.Warning, $"{entity} has duplicate coordinates with another Entity {duplicateCoordEntities.FirstOrDefault().Id}");
                    }
                    else
                    {
                        return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(HasUniqueCoordinates)}");
                    }
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} is Switch, Wall, Path or Canyon node");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"{entity} was null!");
            }
        }

        public static ValidationResult TeleportDestinationCoordinatesAreDifferentToStart(Entity entity)
        {

            if (entity is not null)
            {
                if (entity.IsTeleport() && entity.Position.X == entity.Child && entity.Position.Y == entity.Parent)
                {
                    return new ValidationResult(entity.Id, Result.Warning, $"{entity} has same destination coordinates as start");
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(TeleportDestinationCoordinatesAreDifferentToStart)}");
                }
            }
            else
            {
                return new ValidationResult(0, Result.Fail, $"{entity} was null!");
            }
        }

        public static ValidationResult CheckConnectedWalls(Entity entity, IList<Entity> entities)
        {
            if (entity is not null && entity.IsWall())
            {
                if (entity.Parent == 0)
                {
                    return CheckNextPathEntityChild(entity, entities);

                }
                else if (entity.Child == 0)
                {
                    return CheckNextPathEntityParent(entity, entities);
                }
                else
                {
                    return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(CheckConnectedWalls)}");
                }
            }
            else
            {
                return new ValidationResult(entity != null ? entity.Id: 0, Result.Pass, $"{entity} is not wall");
            }
        }

        private static ValidationResult CheckNextPathEntityChild(Entity entity, IList<Entity> entities)
        {
            var nextWalls = entities?.Where(e => entity.IsWall() && entity.Child > 0 && entity.Child == e.Id);

            if (nextWalls?.Any() ?? false)
            {
                foreach (var wall in nextWalls)
                {
                    if (wall.Parent != entity.Id)
                        return new ValidationResult(wall.Id, Result.Fail, $"{wall} does not have the correct Parent Id, should be: {entity.Id}");

                    if (wall.Child > 0)
                        return CheckNextPathEntityChild(wall, entities);
                }
            }
            return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(CheckConnectedWalls)}");
        }

        private static ValidationResult CheckNextPathEntityParent(Entity entity, IList<Entity> entities)
        {
            var nextWalls = entities?.Where(e => entity.IsWall() && entity.Parent > 0 && entity.Parent == e.Id);

            if (nextWalls?.Any() ?? false)
            {
                foreach (var wall in nextWalls)
                {
                    if (wall.Child != entity.Id)
                        return new ValidationResult(wall.Id, Result.Fail, $"{wall} does not have the correct Child Id, should be: {entity.Id}");

                    if (wall.Parent > 0)
                        return CheckNextPathEntityParent(wall, entities);
                }
            }
            return new ValidationResult(entity.Id, Result.Pass, $"{entity} {nameof(CheckConnectedWalls)}");
        }
    }
}
