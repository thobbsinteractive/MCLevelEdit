using MCLevelEdit.Model.Domain;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.ViewModels.Mappers;

public static class EntitiesToEntityViewModels
{
    public static EntityViewModel[] ToEntityViewModels(this IEnumerable<Entity> entities)
    {
        return entities.Select(e => e.ToEntityViewModel()).ToArray();
    }
}
