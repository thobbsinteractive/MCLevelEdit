namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>A Magic Carpet 2 wizard: a <see cref="Wizard"/> plus MC2's spell settings and Life.</summary>
public class Mc2Wizard : Wizard
{
    // 26 spells per array, in the order of the MC2 Spell enum
    public byte[] StartingSpells { get; set; } = new byte[LevelFormat.MC2_SPELL_COUNT];
    public byte[] SpellLevels { get; set; } = new byte[LevelFormat.MC2_SPELL_COUNT];
    public byte[] BlockedSpells { get; set; } = new byte[LevelFormat.MC2_SPELL_COUNT];
    public short Life { get; set; }
}
