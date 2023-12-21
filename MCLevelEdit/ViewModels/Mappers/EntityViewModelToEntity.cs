﻿using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Domain.Extensions;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntityViewModelToEntity
{
    public static Entity ToEntity(this EntityViewModel entityView)
    {
        var entityType = ((TypeId)entityView.Type).GetEntityFromTypeIdAndModelId(entityView.Model);
        return new Entity() { Id = entityView.Id, EntityType = entityType, Position = new Position(entityView.X, entityView.Y), Parent = entityView.Parent, Child = entityView.Child };
    }
}
