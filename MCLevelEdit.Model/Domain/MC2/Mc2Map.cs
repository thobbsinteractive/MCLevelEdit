namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>A Magic Carpet 2 level: a <see cref="Map"/> plus the fields only MC2 levels have.</summary>
public class Mc2Map : Map
{
    public override GameVersion GameVersion => GameVersion.MagicCarpet2;

    public override MagicCarpet2Terrain.Model.TerrainGame TerrainGame => MagicCarpet2Terrain.Model.TerrainGame.MagicCarpet2;

    public override GameTarget Game => GameTarget.MagicCarpet2;

    /// <summary>Magic Carpet 2's own rules; the Magic Carpet ones are not used here.</summary>
    protected override IEnumerable<ValidationResult> ValidateGameRules() =>
        Validation.MC2.Mc2MapRules.HasPlayerExit(VisibleEntities.ToList());

    public ushort LevelId { get; set; }
    public byte LevelType { get; set; }

    /// <summary>Night levels with level type bit 2 set use the fog palette (PALF-0.DAT) in remc2.</summary>
    public bool UsesFogPalette => (LevelType & 2) != 0;
    /// <summary>Cave roof height. Stored on the terrain generation parameters, which build the cave ceiling from it.</summary>
    public byte BasicHeight
    {
        get => Terrain.GenerationParameters.BasicHeight;
        set => Terrain.GenerationParameters.BasicHeight = value;
    }

    /// <summary>The 8 objective slots.</summary>
    public IList<Objective> Objectives { get; set; } = new List<Objective>();

    /// <summary>The 11 scripted creature behaviour slots.</summary>
    public IList<StageVar> StageVars { get; set; } = new List<StageVar>();

    /// <summary>
    /// The bytes the level was loaded from. When saving, fields the editor does not model yet
    /// are written back from here unchanged.
    /// </summary>
    public byte[]? SourceData { get; set; }

    public Mc2Map()
    {
        // MC2 wizards have their own settings, and MC1's wizard names (Vodor, Gryshnak, ...) do not apply
        Wizards = Enumerable.Range(0, 8)
            .Select(i => (Wizard)new Mc2Wizard
            {
                Name = i == 0 ? "Player" : $"Wizard {i + 1}",
                IsActive = i == 0,
                Agression = 128,
                Perception = 128,
                Reflexes = 128
            })
            .ToArray();
    }

    public IEnumerable<Mc2Wizard> Mc2Wizards => Wizards.OfType<Mc2Wizard>();
}
