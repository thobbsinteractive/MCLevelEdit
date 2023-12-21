using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntityToEntityViewModel
{
    public static EntityViewModel ToEntityViewModel(this Entity entity)
    {
        return new EntityViewModel() { Id = entity.Id, Type = (int)entity.EntityType.TypeId, ModelIdx = 0, X = (byte)entity.Position.X, Y = (byte)entity.Position.Y, Parent = entity.Parent, Child = entity.Child };
    }
}
