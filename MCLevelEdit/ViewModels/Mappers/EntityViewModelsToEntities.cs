using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using MCLevelEdit.Model.Domain;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntityViewModelsToEntities
{
    public static Entity[] ToEntities(this IAvaloniaList<EntityViewModel> entities)
    {
        return entities.Select(e => e.ToEntity()).ToArray();
    }
}