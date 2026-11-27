using MCLevelEdit.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Mc2 = MCLevelEdit.Model.Domain.MC2;

namespace MCLevelEdit.ViewModels.Mappers;

/// <summary>Maps a wizard's Magic Carpet 2 spell arrays to and from the editor's spell rows.</summary>
public static class Mc2SpellsMapper
{
    public static List<Mc2SpellViewModel> ToMc2SpellViewModels(this Mc2.Mc2Wizard wizard) =>
        Enum.GetValues<Mc2.Spell>()
            .Select(spell => (int)spell)
            .Select(i => new Mc2SpellViewModel(i, ((Mc2.Spell)i).ToString(),
                At(wizard.StartingSpells, i), At(wizard.SpellLevels, i), At(wizard.BlockedSpells, i)))
            .ToList();

    public static void ApplyMc2Spells(this Mc2.Mc2Wizard wizard, IReadOnlyList<Mc2SpellViewModel> spells)
    {
        wizard.StartingSpells = spells.Select(s => s.StartingRaw).ToArray();
        wizard.SpellLevels = spells.Select(s => s.LevelRaw).ToArray();
        wizard.BlockedSpells = spells.Select(s => s.BlockedRaw).ToArray();
    }

    private static byte At(byte[] values, int index) => index < values.Length ? values[index] : (byte)0;
}
