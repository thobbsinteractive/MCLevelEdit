using Mc1Effect = MCLevelEdit.Model.Domain.MC1.Effect;
using Mc2Effect = MCLevelEdit.Model.Domain.MC2.Effect;
using Mc2Special = MCLevelEdit.Model.Domain.MC2.Special;
using Mc2Switch = MCLevelEdit.Model.Domain.MC2.Switch;

namespace MCLevelEdit.Model.Domain;

/// <summary>The fields of an entity that a tooltip can explain.</summary>
public enum EntityField
{
    X,
    Y,
    DisId,
    SwitchSize,
    SwitchId,
    Parent,
    Child,
    Par3
}

/// <summary>
/// What each field of an entity means. Most fields are generic, but the terrain features (walls, paths, canyons,
/// caves, gates...) use them for their own values, as found while porting the game's terrain code.
/// </summary>
public static class EntityFieldHelp
{
    private const string LinkBefore = "The ID of the node linked before this one (0: this is the first node of the line)";
    private const string LinkAfter = "The ID of the node linked after this one (0: this is the last node of the line)";
    private const string StageTag = "The stage tag: a line of nodes is made from its first node with a tag (Magic Carpet 2 tags every line with no switch)";

    public static string Get(GameVersion game, TypeId type, int model, EntityField field) =>
        GetSpecific(game, type, model, field) ?? GetGeneric(game, field);

    private static string GetGeneric(GameVersion game, EntityField field) => field switch
    {
        EntityField.X => "The square across the map, 0-255",
        EntityField.Y => "The square down the map, 0-255",
        EntityField.DisId => "The switch that triggers this entity (65535: no switch, it is there from the start)",
        EntityField.SwitchSize => "The size of the switch's area",
        EntityField.SwitchId => "The switch's ID",
        EntityField.Parent => "The ID of the entity linked before this one",
        EntityField.Child => "The ID of the entity linked after this one",
        EntityField.Par3 => game == GameVersion.MagicCarpet2 ? "An extra value some Magic Carpet 2 entities use" : "Not used by Magic Carpet",
        _ => string.Empty
    };

    private static string? GetSpecific(GameVersion game, TypeId type, int model, EntityField field)
    {
        bool mc2 = game == GameVersion.MagicCarpet2;
        if (type == TypeId.Special && mc2)
        {
            switch ((Mc2Special)model)
            {
                case Mc2Special.WallGate:
                    return field switch
                    {
                        EntityField.Parent => "The gate's direction: 0 along x, 1 along y",
                        EntityField.Child => "The gate's length in squares",
                        _ => null
                    };
                case Mc2Special.CaveDoor:
                    return field switch
                    {
                        EntityField.Parent => "The door's direction: 0 along x, otherwise along y",
                        EntityField.Par3 => "The door's size",
                        _ => null
                    };
            }
            return null;
        }

        if (type == TypeId.Switch && mc2)
        {
            switch ((Mc2Switch)model)
            {
                case Mc2Switch.ExitLevel:
                case Mc2Switch.ExitLevel2:
                    string gate = (Mc2Switch)model == Mc2Switch.ExitLevel ? "ExitGate (Special 3)" : "ExitGate2 (Special 4)";
                    return field switch
                    {
                        EntityField.DisId => $"The switch that opens the exit, usually OnVictory's, shared with the {gate}",
                        EntityField.SwitchSize => $"The exit's size: a wizard who flies into it ends the level, flying to the {gate}",
                        EntityField.SwitchId => "Not fired: the game ends the level instead",
                        _ => null
                    };
                case Mc2Switch.StageComplete:
                    return field switch
                    {
                        EntityField.Par3 => "The stage that must be complete for this switch to fire",
                        _ => null
                    };
            }
            return null;
        }

        if (type != TypeId.Effect)
            return null;

        // IDs shared by both games
        switch ((Mc1Effect)model)
        {
            case Mc1Effect.Wall:
            case Mc1Effect.Path:
            case Mc1Effect.RidgeNode:
                return field switch
                {
                    EntityField.Parent => LinkBefore,
                    EntityField.Child => LinkAfter,
                    EntityField.SwitchId => StageTag,
                    _ => null
                };
            case Mc1Effect.Canyon:
                return field switch
                {
                    EntityField.Parent => LinkBefore,
                    EntityField.Child => LinkAfter,
                    EntityField.SwitchId => StageTag,
                    EntityField.Par3 when mc2 => "The canyon's depth at this node: 0-3 dig 2, 6, 16 or 32 deep, any other value that deep",
                    _ => null
                };
            case Mc1Effect.VillagerBuilding:
                return field switch
                {
                    EntityField.Parent => "The building's shape",
                    _ => null
                };
        }

        if (!mc2)
            return null;

        switch ((Mc2Effect)model)
        {
            case Mc2Effect.Volcano:
            case Mc2Effect.Crater:
            case Mc2Effect.Earthquake:
                return field switch
                {
                    EntityField.Parent => "The spell's level, 0-2: it sets the size or how long it lasts",
                    _ => null
                };
            case Mc2Effect.TunnelNode:
                return field switch
                {
                    EntityField.Parent => LinkBefore,
                    EntityField.Child => LinkAfter,
                    EntityField.SwitchId => StageTag,
                    EntityField.Par3 => "The tunnel's width at this node",
                    _ => null
                };
            case Mc2Effect.TunnelCarver:
                return null;
            case Mc2Effect.CaveRoom:
                return field switch
                {
                    EntityField.Parent => "Half the room's width, in squares",
                    EntityField.Child => "Half the room's height, in squares",
                    EntityField.Par3 => "The room's depth",
                    _ => null
                };
            case Mc2Effect.CaveMound:
                return field switch
                {
                    EntityField.SwitchSize => "The mound's radius",
                    _ => null
                };
            case Mc2Effect.Stalactite:
            case Mc2Effect.Stalagmite:
                return field switch
                {
                    EntityField.SwitchSize => "The cone's radius",
                    EntityField.Par3 => "The cone's length",
                    _ => null
                };
        }
        return null;
    }
}
