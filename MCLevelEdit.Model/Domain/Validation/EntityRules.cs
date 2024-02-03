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
                        return new ValidationResult(entity.Id, Result.Warning, nameof(HasSwitch));
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
    }
}
