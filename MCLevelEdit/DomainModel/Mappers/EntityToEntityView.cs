using MCLevelEdit.ViewModels;

namespace MCLevelEdit.DataModel.Mappers;

public static class EntityToEntityView
{
    public static EntityView ToEntityView(this Entity entity)
    {
        return new EntityView() { Id = entity.Id, Type = (int)entity.EntityType.TypeId, ModelIdx = 0, X = entity.Position.X, Y = entity.Position.Y, Parent = entity.Parent, Child = entity.Child };
    }
}
