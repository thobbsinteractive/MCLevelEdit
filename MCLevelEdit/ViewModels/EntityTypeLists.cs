using Avalonia.Collections;
using MCLevelEdit.Model.Domain;
using MCLevelEdit.Model.Repository;
using System.Collections.Generic;
using System.Linq;

namespace MCLevelEdit.ViewModels;

/// <summary>
/// Entity type lists for dropdowns, matching the game of the open map. The lists are shared and
/// updated in place, so every bound dropdown follows when a level from the other game is opened.
/// </summary>
public static class EntityTypeLists
{
    private static GameVersion? _game;

    /// <summary>The game's entity types.</summary>
    public static AvaloniaList<KeyValuePair<int, string>> TypeIds { get; } = new();

    /// <summary>The game's entity types, with an empty "all types" entry (key 0) first, for filters.</summary>
    public static AvaloniaList<KeyValuePair<int, string>> TypeIdsWithAll { get; } = new();

    static EntityTypeLists() => Refresh();

    public static void Refresh()
    {
        var game = MapRepository.Map?.GameVersion ?? GameVersion.MagicCarpet1;
        if (_game == game)
            return;
        _game = game;

        var types = EntityCatalog.GetTypeIds(game)
            .Select(t => new KeyValuePair<int, string>((int)t, t.ToString()))
            .ToList();

        TypeIds.Clear();
        TypeIds.AddRange(types);

        TypeIdsWithAll.Clear();
        TypeIdsWithAll.Add(new KeyValuePair<int, string>(0, ""));
        TypeIdsWithAll.AddRange(types);
    }
}
