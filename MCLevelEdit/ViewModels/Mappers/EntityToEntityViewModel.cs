using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntityToEntityViewModel
{
    public static EntityViewModel ToEntityViewModel(this Entity entity)
    {
        return new EntityViewModel() {
            Id = entity.Id,
            Type = (int)entity.EntityType.TypeId,
            Model = entity.EntityType.Model.Id,
            X = (byte)entity.Position.X,
            Y = (byte)entity.Position.Y,
            DisId = entity.DisId,
            SwitchSize = entity.SwitchSize, SwitchId = entity.SwitchId,
            Parent = entity.Parent,
            Child = entity.Child
        };
    }
}
