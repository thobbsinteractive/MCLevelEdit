namespace MCLevelEdit.Model.Domain.MC2;

/// <summary>
/// One of the 11 Magic Carpet 2 scripted creature behaviours (type_str_0x3647Ac in remc2, 8 bytes).
/// Entities are linked to these through their stage tag. Slot 0 is unused by the game.
/// </summary>
public class StageVar
{
    /// <summary>Low nibble is the behaviour type; bits 0x10, 0x20, 0x40 and 0x80 are flags (see InitStageVars_11EE0).</summary>
    public byte TypeAndFlags { get; set; }

    /// <summary>Index of the stage var that follows this one, or 0.</summary>
    public byte NextStage { get; set; }

    public ushort EntityIndex { get; set; }
    public ushort TargetX { get; set; }
    public ushort TargetY { get; set; }

    public int Type => TypeAndFlags & 0x0F;

    public StageVar Copy() => new StageVar
    {
        TypeAndFlags = TypeAndFlags,
        NextStage = NextStage,
        EntityIndex = EntityIndex,
        TargetX = TargetX,
        TargetY = TargetY
    };
}
