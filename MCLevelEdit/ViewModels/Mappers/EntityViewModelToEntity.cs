using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;
using MCLevelEdit.Model.Domain.MC2;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntityViewModelToEntity
{
    public static Entity ToEntity(this EntityViewModel entityView)
    {
        var entityType = ((TypeId)entityView.Type).GetEntityTypeOrUnknown(entityView.Model);

        // MC2 entities carry an extra field
        Entity entity = entityType.Game == GameVersion.MagicCarpet2
            ? new Mc2Entity { Par3 = entityView.Par3 }
            : new Entity();

        entity.Id = entityView.Id;
        entity.EntityType = entityType;
        entity.Position = new Position(entityView.X, entityView.Y);
        entity.DisId = entityView.DisId;
        entity.SwitchSize = entityView.SwitchSize;
        entity.SwitchId = entityView.SwitchId;
        entity.Parent = entityView.Parent;
        entity.Child = entityView.Child;
        return entity;
    }
}
