using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntityViewModelToEntity
{
    public static Entity ToEntity(this EntityViewModel entityView)
    {
        var entityType = ((TypeId)entityView.Type).GetEntityTypeFromTypeIdAndModelId(entityView.Model);
        return new Entity() 
        { 
            Id = entityView.Id, 
            EntityType = entityType, 
            Position = new Position(entityView.X, entityView.Y), 
            DisId = entityView.DisId, 
            SwitchSize = entityView.SwitchSize, 
            SwitchId = entityView.SwitchId, 
            Parent = entityView.Parent, 
            Child = entityView.Child,
        };
    }
}
